namespace Parser.Tokenizers.Position
{
    public interface IPosition
    {
        // static IPosition UNKNOWN = new IPosition()
        // {
        //     public int Line => 0;
        //     public int Pos  => 0;
        // };

        int Line { get; }
        int Position { get; }
    }
}