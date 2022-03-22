namespace Parser.Eval.Expressions
{
    public class VariableReference : Expression
    {
        private readonly Variable Variable;

        public VariableReference(Variable variable) : base(false)
        {
            Variable = variable;
        }

        public override bool Equals(Expression? other)
        {
            return other is VariableReference variableReference && Variable.Equals(variableReference.Variable);
        }

        public override double Evaluate()
        {
            return Variable.Value;
        }

        public override Expression Simplify()
        {
            return Variable.IsConstant ? new Constant(Variable.Value) : this;
        }

        public override string ToString()
        {
            return Variable.Name;
        }
    }
}