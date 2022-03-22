namespace Parser.Tokenizers.Position
{
    public class CharPosition : IPosition
    {
        public char Value { get; }
        public int Line { get; }
        public int Position { get; }

        public bool IsDigit => char.IsDigit(Value);
        public bool IsLetter => char.IsLetter(Value);
        public bool IsWhitespace => char.IsWhiteSpace(Value) && !IsEndOfInput;
        public bool IsNewline => Value == '\n';
        public bool IsEndOfInput => Value == '\0';

        public string StringValue => IsEndOfInput ? "" : Value.ToString();

        public CharPosition(char value, int line, int position)
        {
            Value = value;
            Line = line;
            Position = position;
        }

        public bool Is(params char[] tests)
        {
            return tests.Any(test => test == Value && test != '\0');
        }

        public override string ToString()
        {
            return IsEndOfInput ? "<End of Input>" : Value.ToString();
        }
    }
}