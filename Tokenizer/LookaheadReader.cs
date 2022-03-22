namespace Parser.Tokenizers
{
    public class LookaheadReader : Lookahead<Position.CharPosition>, IDisposable
    {
        private readonly Stream Input;
        private int Line = 1;
        private int Position;

        public LookaheadReader(Stream input)
        {
            if (input is null) { throw new ArgumentException("Parameter 'input' must not be null."); }
            Input = new BufferedStream(input);
        }

        protected override Position.CharPosition EndOfInput => new('\0', Line, Position);

        protected override bool TryFetch(out Position.CharPosition item)
        {
            item = new Position.CharPosition('\0', 0, 0);
            try
            {
                int character = Input.ReadByte();
                if (character == -1) { return false; }

                Position.CharPosition result = new((char)character, Line, ++Position);
                if (character == '\n')
                {
                    Line++;
                    Position = 0;
                }
                item = result;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override string ToString()
        {
            if (ItemBuffer.Count == 0)
            {
                return $"{Line}:{Position}: Buffer empty";
            }
            else if (ItemBuffer.Count < 2)
            {
                return $"{Line}:{Position}: {Current}";
            }
            return $"{Line}:{Position}: {Current}, {Next}";
        }

        public void Dispose()
        {
            Input.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}