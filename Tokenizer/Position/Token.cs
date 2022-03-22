namespace Parser.Tokenizers.Position
{
    public class Token : IPosition
    {
        public enum TokenType
        {
            ID,
            Decimal,
            ScientificDecimal,
            Integer,
            Symbol,
            EOI,
        }

        public TokenType Type { get; private set; }
        public string Contents { get; private set; } = "";
        public string Source { get; private set; } = "";
        private string? InternalTrigger;
        private string _trigger = "";

        public static readonly Token EMPTY = Create(TokenType.EOI, new CharPosition('\0', 0, 0));

        public string Trigger
        {
            get
            {
                if (InternalTrigger is null) { InternalTrigger = _trigger; }
                return InternalTrigger;
            }
        }

        public void SetContent(string content)
        {
            Contents = content;
        }

        public void SetTrigger(string trigger)
        {
            _trigger = trigger;
        }

        public void SetSource(string source)
        {
            Source = source;
        }

        public bool IsEnd => Type == TokenType.EOI;
        public bool IsInteger => Is(TokenType.Integer);
        public bool IsDecimal => Is(TokenType.Decimal);
        public bool IsScientificDecimal => Is(TokenType.ScientificDecimal);
        public bool IsNumber => IsInteger || IsDecimal || IsScientificDecimal;

        public int Line { get; private set; }
        public int Position { get; private set; }

        private Token() { }

        public static Token Create(TokenType type, IPosition pos)
        {
            return new() { Type = type, Line = pos.Line, Position = pos.Position };
        }

        public static Token CreateAndFill(TokenType type, CharPosition ch)
        {
            return new()
            {
                Type = type,
                Line = ch.Line,
                Position = ch.Position,
                Contents = ch.StringValue,
                _trigger = ch.StringValue,
                Source = ch.ToString()
            };
        }

        public Token AddToTrigger(CharPosition ch)
        {
            _trigger += ch.Value;
            InternalTrigger = null;
            Source += ch.Value;
            return this;
        }

        public Token AddToSource(CharPosition ch)
        {
            Source += ch.Value;
            return this;
        }

        public Token AddToContent(CharPosition ch)
        {
            return AddToContent(ch.Value);
        }

        public Token AddToContent(char ch)
        {
            Contents += ch;
            Source += ch;
            return this;
        }

        public Token AddToContentSilent(char ch)
        {
            Contents += ch;
            return this;
        }

        public bool Matches(TokenType type, string trigger)
        {
            if (!Is(type)) { return false; }
            return trigger is null ? throw new ArgumentNullException(nameof(trigger)) : Trigger == trigger;
        }

        public bool WasTriggeredBy(params string[] triggers)
        {
            return triggers.Any(trigger => trigger is not null && trigger == Trigger);
        }

        public bool HasContent(string content)
        {
            return content is null ? throw new ArgumentNullException(nameof(content)) : content.ToLower() == Contents.ToLower();
        }

        public bool Is(TokenType type)
        {
            return type == Type;
        }

        public bool IsSymbol(params string[] symbols)
        {
            return symbols.Length == 0 ? Is(TokenType.Symbol) : symbols.Any(symbol => Matches(TokenType.Symbol, symbol));
        }

        public bool IsIdentifier(params string[] values)
        {
            return values.Length == 0
                ? Is(TokenType.ID)
                : values.Any(value => Matches(TokenType.ID, value));
        }

        public override string ToString()
        {
            return $"{GetType()}:{Source} ({Line}:{Position})";
        }
    }
}