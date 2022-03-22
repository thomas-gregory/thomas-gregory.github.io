namespace Parser.Eval.Functions
{
    public abstract class Function
    {
        public bool IsDeterministic;
        public abstract int ArgumentNumber { get; }

        public double Evaluate(IList<Expressions.Expression> arguments)
        {
            if (ArgumentNumber == -1 || ArgumentNumber == arguments.Count)
            {
                return EvaluateInternal(arguments);
            }

            throw new ArgumentException($"Expected {ArgumentNumber} arguments, but received {arguments.Count}", nameof(arguments));
        }

        protected abstract double EvaluateInternal(IList<Expressions.Expression> arguments);
    }
}