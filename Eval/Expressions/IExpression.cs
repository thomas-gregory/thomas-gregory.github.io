namespace Parser.Eval.Expressions
{
    public abstract class Expression : IEquatable<Expression>
    {
        public readonly bool IsConstant;

        protected Expression(bool isConstant)
        {
            IsConstant = isConstant;
        }

        public abstract double Evaluate();
        public virtual Expression Simplify()
        {
            return this;
        }

        public sealed override bool Equals(object? obj)
        {
            return obj is Expression expression && GetType() == expression.GetType() && Equals(expression);
        }

        public abstract bool Equals(Expression? other);

        public override int GetHashCode()
        {
            return HashCode.Combine(IsConstant);
        }
    }
}