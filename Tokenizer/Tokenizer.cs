using Parser.Tokenizers.Position;

namespace Parser.Tokenizers
{
    public class Tokenizer : Lookahead<Token>
    {
        protected LookaheadReader Input;

        private const char DecimalSeparator = '.';
        private const char EffectiveDecimalSeparator = '.';
        private const char GroupingSeparator = '_';
        private const char ScientificNotationSeparator = 'e';
        private const char AlternateScientificNotationSeparator = 'E';
        private const char EffectiveScientificNotationSeparator = 'E';

        private const string LineComment = "//";
        private const string BlockCommentStart = "/*";
        private const string BlockCommentEnd = "*/";
        private readonly char[] Brackets = { '(', '[', '{', '}', ']', ')' };
        private const bool TreatSinglePipeAsBracket = true;

        public bool More => !Current.IsEnd;
        public bool AtEnd => Current.IsEnd;
        protected override Token EndOfInput => Token.CreateAndFill(Token.TokenType.EOI, Input.Current);

        public Tokenizer(Stream input)
        {
            Input = new LookaheadReader(input);
        }

        protected override bool TryFetch(out Token item)
        {
            item = Token.EMPTY;
            while (Input.Current.IsWhitespace) { Input.Consume(); }

            if (Input.Current.IsEndOfInput) { return false; }

            if (IsAtStartOfLineComment(true))
            {
                SkipToEndOfLine();
                return TryFetch(out item);
            }

            if (IsAtStartOfBlockComment(true))
            {
                SkipBlockComment();
                return TryFetch(out item);
            }

            if (IsAtStartOfNumber)
            {
                item = FetchNumber();
                return true;
            }

            if (IsAtStartOfIdentifier)
            {
                item = FetchID();
                return true;
            }

            if (IsAtBracket(false))
            {
                item = Token.CreateAndFill(Token.TokenType.Symbol, Input.Consume());
                return true;
            }

            if (IsSymbolCharacter(Input.Current))
            {
                item = FetchSymbol();
                return true;
            }

            throw new ArgumentException($"Invalid character in input: '{Input.Current.StringValue}'");
            // return TryFetch(out item);
        }

        protected bool IsAtStartOfIdentifier => Input.Current.IsLetter;
        protected bool IsAtStartOfNumber => Input.Current.IsDigit
            || Input.Current.Is('-') && Input.Next.IsDigit
            || Input.Current.Is('-') && Input.Next.Is('.') && Input.GetNext(2).IsDigit
            || Input.Current.Is('.') && Input.Next.IsDigit;

        protected bool IsAtBracket(bool InSymbol)
            => Input.Current.Is(Brackets) ||
               !InSymbol &&
                TreatSinglePipeAsBracket &&
                Input.Current.Is('|') &&
               !Input.Next.Is('|');

        protected bool CanConsumeThisString(string str, bool consume)
        {
            if (str is null) { return false; }

            for (int i = 0; i < str.Length; i++)
            {
                if (!Input.GetNext(i).Is(str.ElementAt(i))) { return false; }
            }

            if (consume) { Input.Consume(str.Length); }
            return true;
        }

        protected bool IsAtStartOfLineComment(bool consume)
        {
            return CanConsumeThisString(LineComment, consume);
        }

        protected void SkipToEndOfLine()
        {
            while (!Input.Current.IsEndOfInput && !Input.Current.IsNewline)
            {
                Input.Consume();
            }
        }

        protected bool IsAtStartOfBlockComment(bool consume)
            => CanConsumeThisString(BlockCommentStart, consume);

        protected bool IsAtEndOfBlockComment()
            => CanConsumeThisString(BlockCommentEnd, true);

        protected void SkipBlockComment()
        {
            while (!Input.Current.IsEndOfInput)
            {
                if (IsAtEndOfBlockComment()) { return; }
                Input.Consume();
            }
            throw new ArgumentException("Premature end of block comment.");
        }

        public bool HandleStringEscape(char separator, char escapeChar, Token stringToken)
        {
            if (Input.Current.Is(separator))
            {
                stringToken.AddToContent(separator);
                stringToken.AddToSource(Input.Consume());
                return true;
            }
            else if (Input.Current.Is(escapeChar))
            {
                stringToken.AddToContentSilent(escapeChar);
                stringToken.AddToSource(Input.Consume());
                return true;
            }
            else if (Input.Current.Is('n'))
            {
                stringToken.AddToContentSilent('\n');
                stringToken.AddToSource(Input.Consume());
                return true;
            }
            else if (Input.Current.Is('r'))
            {
                stringToken.AddToContentSilent('\r');
                stringToken.AddToSource(Input.Consume());
                return true;
            }
            else
            {
                return false;
            }
        }

        protected Token FetchID()
        {
            Token result = Token.Create(Token.TokenType.ID, Input.Current);
            result.AddToContent(Input.Consume());

            while (IsIdentifierChar(Input.Current))
            {
                result.AddToContent(Input.Consume());
            }

            return result;
        }

        protected static bool IsIdentifierChar(CharPosition current)
        {
            return current.IsDigit || current.IsLetter || current.Is('_');
        }

        protected Token FetchSymbol()
        {
            Token result = Token.Create(Token.TokenType.Symbol, Input.Current);
            result.AddToTrigger(Input.Consume());
            if (result.IsSymbol("&") && Input.Current.Is('&')
             || result.IsSymbol("|") && Input.Current.Is('|')
             || result.IsSymbol() && Input.Current.Is('='))
            {
                result.AddToTrigger(Input.Consume());
            }
            return result;
        }

        protected bool IsSymbolCharacter(CharPosition ch)
        {
            if (ch.IsEndOfInput || ch.IsDigit || ch.IsLetter || ch.IsWhitespace)
            {
                return false;
            }

            char c = ch.Value;
            if (char.IsControl(c)) { return false; }

            return !(IsAtBracket(true)
                  || IsAtStartOfBlockComment(false)
                  || IsAtStartOfLineComment(false)
                  || IsAtStartOfNumber
                  || IsAtStartOfIdentifier
            );
        }

        protected Token FetchNumber()
        {
            Token result = Token.Create(Token.TokenType.Integer, Input.Current);
            result.AddToContent(Input.Consume());
            while (Input.Current.IsDigit
                || Input.Current.Is(DecimalSeparator)
                || Input.Current.Is(GroupingSeparator) && Input.Next.IsDigit
                || (Input.Current.Is(ScientificNotationSeparator, AlternateScientificNotationSeparator)
                    && (Input.Next.IsDigit || Input.Next.Is('+', '-')))
            )
            {
                if (Input.Current.Is(GroupingSeparator))
                {
                    result.AddToSource(Input.Consume());
                }
                else if (Input.Current.Is(DecimalSeparator))
                {
                    if (result.Is(Token.TokenType.Decimal) || result.Is(Token.TokenType.ScientificDecimal))
                    {
                        throw new ArgumentException("Unexpected decimal separators");
                    }
                    else
                    {
                        Token decimalToken = Token.Create(Token.TokenType.Decimal, result);
                        decimalToken.SetContent(result.Contents + EffectiveDecimalSeparator);
                        decimalToken.SetSource(result.Source);
                        result = decimalToken;
                    }
                    result.AddToSource(Input.Consume());
                }
                else if (Input.Current.Is(ScientificNotationSeparator, AlternateScientificNotationSeparator))
                {
                    if (result.Is(Token.TokenType.ScientificDecimal))
                    {
                        throw new ArgumentException("Unexpected scientific notation separators.");
                    }
                    else
                    {
                        Token scientificDecimalToken = Token.Create(Token.TokenType.ScientificDecimal, result);
                        scientificDecimalToken.SetContent(result.Contents + EffectiveScientificNotationSeparator);
                        scientificDecimalToken.SetSource(result.Source + EffectiveScientificNotationSeparator);
                        result = scientificDecimalToken;
                        Input.Consume();
                        if (Input.Current.Is('+', '-'))
                        {
                            result.AddToContent(Input.Consume());
                        }
                    }
                }
                else
                {
                    result.AddToContent(Input.Consume());
                }
            }

            return result;
        }

        public override string ToString()
        {
            if (ItemBuffer.Count == 0) { return "No token fetched."; }
            if (ItemBuffer.Count < 2) { return "Current: " + Current; }
            return $"Current: {Current}, Next: {Next}";
        }

        public void ConsumeExpectedSymbol(string symbol)
        {
            if (Current.Matches(Token.TokenType.Symbol, symbol)) { Consume(); }
            else { throw new ArgumentException($"Unexpected token: '{Current.Source}'. Expected: '{symbol}'"); }
        }
    }
}