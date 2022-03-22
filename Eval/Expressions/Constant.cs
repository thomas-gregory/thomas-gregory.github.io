namespace Parser.Eval.Expressions
{
    public class Constant : Expression
    {
        private readonly double Value;
        public static readonly Constant EMPTY = new(double.NaN);

        public Constant(double value) : base(true)
        {
            Value = value;
        }

        public override double Evaluate()
        {
            return Value;
        }

        public override Expression Simplify()
        {
            return this;
        }

        public override bool Equals(Expression? other)
        {
            return other is Constant constant && Value == constant.Value;
        }
    }
}