using Parser.Eval.Functions;

namespace Parser.Eval
{
    public class Scope
    {
        private readonly Scope? Parent;
        private readonly Dictionary<string, Variable> VariableContext = new();
        private readonly Dictionary<string, Function> FunctionContext = new();

        public static readonly Scope Root = new(true);

        public IEnumerable<string> LocalNames => VariableContext.Keys.Union(FunctionContext.Keys);
        public IEnumerable<Variable> LocalVariables => VariableContext.Values;
        public IEnumerable<Function> LocalFunctions => FunctionContext.Values;

        public IEnumerable<string> Names
        {
            get
            {
                IEnumerable<string> result = LocalNames;
                if (Parent is null) { return result; }
                return result.Concat(Parent.Names);
            }
        }
        public IEnumerable<Variable> Variables
        {
            get
            {
                IEnumerable<Variable> result = LocalVariables;
                if (Parent is null) { return result; }
                return result.Concat(Parent.Variables);
            }
        }
        public IEnumerable<Function> Functions
        {
            get
            {
                IEnumerable<Function> result = LocalFunctions;
                if (Parent is null) { return result; }
                return result.Concat(Parent.Functions);
            }
        }

        public Scope() : this(false) { }

        private Scope(bool skipParent)
        {
            if (!skipParent) { Parent = Root; }
        }

        public bool TryFindVariable(string name, out Variable variable)
        {
            if (VariableContext.TryGetValue(name, out variable!)) { return true; }
            if (Parent is not null) { return Parent.TryFindVariable(name, out variable); }

            return false;
        }

        public bool TryFindFunction(string name, out Function function)
        {
            if (FunctionContext.TryGetValue(name, out function!)) { return true; }
            if (Parent is not null) { return Parent.TryFindFunction(name, out function); }

            return false;
        }

        public Variable? GetVariable(string name)
        {
            if (TryFindVariable(name, out Variable result)) { return result; }
            return null;
        }

        public Function? GetFunction(string name)
        {
            if (TryFindFunction(name, out Function result)) { return result; }
            return null;
        }

        public Variable CreateConstant(string name, double value)
        {
            if (VariableContext.TryGetValue(name, out Variable? result)) { result.MakeConstant(value); return result; }
            result = new Variable(name);
            result.MakeConstant(value);
            VariableContext.Add(name, result);

            return result;
        }

        public Variable CreateVariable(string name)
        {
            if (VariableContext.TryGetValue(name, out Variable? result)) { return result; }
            result = new(name);
            VariableContext.Add(name, result);

            return result;
        }

        public Function CreateFunction(string name, Function function)
        {
            if (FunctionContext.TryGetValue(name, out Function? result)) { return result; }
            FunctionContext.Add(name, function);

            return function;
        }

        public void RemoveVariable(string name)
        {
            if (VariableContext.TryGetValue(name, out _)) { VariableContext.Remove(name); }
        }

        public void RemoveFunction(string name)
        {
            if (FunctionContext.TryGetValue(name, out _)) { FunctionContext.Remove(name); }
        }
    }
}