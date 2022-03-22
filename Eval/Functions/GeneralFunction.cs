using Parser.Eval.Expressions;

namespace Parser.Eval.Functions
{
    public class GeneralFunction : Function
    {
        public override int ArgumentNumber => _argumentNumber;

        private readonly int _argumentNumber;
        private readonly Func<IList<Expression>, double> Function;

        public GeneralFunction(Func<IList<Expression>, double> function, int argumentNumber = -1, bool isDeterministic = false)
        {
            Function = function;
            _argumentNumber = argumentNumber;
            IsDeterministic = isDeterministic;
        }

        protected override double EvaluateInternal(IList<Expression> arguments)
        {
            return Function(arguments);
        }
    }
}