namespace Parser.Eval.Functions
{
    public class UnaryFunction : Function
    {
        public override int ArgumentNumber => 1;

        protected Func<double, double> Function;

        public UnaryFunction(Func<double, double> function, bool isDeterministic = false)
        {
            Function = function;
            IsDeterministic = isDeterministic;
        }

        protected override double EvaluateInternal(IList<Expressions.Expression> arguments)
        {
            double argument = arguments[0].Evaluate();
            if (double.IsNaN(argument)) { return double.NaN; }
            return Function(argument);
        }
    }
}