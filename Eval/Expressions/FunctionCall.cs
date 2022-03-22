namespace Parser.Eval.Expressions
{
    public class FunctionCall : Expression
    {
        private readonly Functions.Function Function;
        private readonly IList<Expression> Arguments;
        public int ArgumentsNumber => Arguments.Count;

        public FunctionCall(Functions.Function function, IList<Expression> arguments) : base(false)
        {
            Function = function;
            Arguments = arguments;
        }

        public void AddArgument(Expression argument)
        {
            Arguments.Add(argument);
        }

        public override double Evaluate()
        {
            return Function.Evaluate(Arguments);
        }

        public override Expression Simplify()
        {
            if (!Function.IsDeterministic || Arguments.Any(x => !x.IsConstant)) { return this; }
            return new Constant(Evaluate());
        }

        public override bool Equals(Expression? other)
        {
            return other is FunctionCall functionCall && Arguments == functionCall.Arguments && Function == functionCall.Function;
        }
    }
}