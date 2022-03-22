namespace Parser.Eval.Expressions
{
    public class BinaryOperation : Expression
    {
        // High bit is precedence, low bit is unique identifier.
        public enum Operations : byte
        {
            And = 0x11,
            Or = 0x12,
            LessThan = 0x23,
            LessThanOrEqual = 0x24,
            Equal = 0x25,
            GreaterThanOrEqual = 0x26,
            GreaterThan = 0x27,
            NotEqual = 0x28,
            Add = 0x39,
            Subtract = 0x3A,
            Multiply = 0x4B,
            Divide = 0x4C,
            Modulo = 0x4D,
            Power = 0x5E,
        }

        public readonly Operations Operation;
        public Expression Left;
        public Expression Right;

        public bool IsSealed { get; private set; }

        public BinaryOperation(Operations operation, Expression left, Expression right) : base(false)
        {
            Operation = operation;
            Left = left;
            Right = right;
        }

        public static int GetPriority(Operations operation)
        {
            return (((int)operation) >> 4) & 0xF;
        }

        public void Seal()
        {
            IsSealed = true;
        }

        public override double Evaluate()
        {
            double a = Left.Evaluate();
            double b = Right.Evaluate();

            return Operation switch
            {
                Operations.Add => a + b,
                Operations.Subtract => a - b,
                Operations.Multiply => a * b,
                Operations.Divide => a / b,
                Operations.Power => Math.Pow(a, b),
                Operations.Modulo => a % b,

                Operations.LessThan => a < b ? 1 : 0,
                Operations.LessThanOrEqual => a < b || Math.Abs(a - b) < double.Epsilon ? 1 : 0,
                Operations.GreaterThan => a > b ? 1 : 0,
                Operations.GreaterThanOrEqual => a > b || Math.Abs(a - b) < double.Epsilon ? 1 : 0,
                Operations.Equal => Math.Abs(a - b) < double.Epsilon ? 1 : 0,
                Operations.NotEqual => Math.Abs(a - b) > double.Epsilon ? 1 : 0,

                Operations.And => Math.Abs(a) > double.Epsilon &&
                                  Math.Abs(b) > double.Epsilon ? 1 : 0,
                Operations.Or => Math.Abs(a) > double.Epsilon ||
                                 Math.Abs(b) > double.Epsilon ? 1 : 0,

                _ => throw new ArgumentException(Operation.ToString())
            };
        }

        public override Expression Simplify()
        {
            Left = Left.Simplify();
            Right = Right.Simplify();

            if (Left.IsConstant && Right.IsConstant) { return new Constant(Evaluate()); }

            if (Operation is Operations.Add or Operations.Multiply)
            {
                if (Right.IsConstant) { (Left, Right) = (Right, Left); }
                if (Right is BinaryOperation)
                {
                    if (TrySimplifyRightSide(out Expression childOp)) { return childOp; }
                }
            }

            return this;
        }

        public bool TrySimplifyRightSide(out Expression simplified)
        {
            simplified = new Constant(0);

            BinaryOperation childOp = (BinaryOperation)Right;
            if (Operation != childOp.Operation) { return false; }

            if (Left.IsConstant && childOp.Left.IsConstant)
            {
                if (Operation == Operations.Add)
                {
                    simplified = new BinaryOperation(Operation, new Constant(Left.Evaluate() + childOp.Left.Evaluate()), childOp.Right);
                    return true;
                }
                else if (Operation == Operations.Multiply)
                {
                    simplified = new BinaryOperation(Operation, new Constant(Left.Evaluate() * childOp.Left.Evaluate()), childOp.Right);
                    return true;
                }
            }

            if (childOp.Left.IsConstant)
            {
                simplified = new BinaryOperation(Operation, childOp.Left, new BinaryOperation(Operation, Left, childOp.Right));
                return true;
            }

            return false;
        }

        public override bool Equals(Expression? other)
        {
            return other is BinaryOperation binaryOperation &&
                Operation == binaryOperation.Operation &&
                Left.Equals(binaryOperation.Left) &&
                Right.Equals(binaryOperation.Right);
        }

        public override string ToString()
        {
            return $"({Left} {Operation} {Right})";
        }
    }
}