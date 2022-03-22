namespace Parser.Eval.Functions
{
    public class BinaryFunction : Function
    {
        public override int ArgumentNumber => 1;

        protected Func<double, double, double> Function;

        public BinaryFunction(Func<double, double, double> function, bool isDeterministic = false)
        {
            Function = function;
            IsDeterministic = isDeterministic;
        }

        protected override double EvaluateInternal(IList<Expressions.Expression> arguments)
        {
            double argument1 = arguments[0].Evaluate();
            if (double.IsNaN(argument1)) { return double.NaN; }

            double argument2 = arguments[1].Evaluate();
            if (double.IsNaN(argument2)) { return double.NaN; }

            return Function(argument1, argument2);
        }
    }
}