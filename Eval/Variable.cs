namespace Parser.Eval
{
    public class Variable
    {
        public string Name { get; private set; }
        public bool IsConstant { get; private set; }

        public double Value { get; private set; }

        public Variable(string name)
        {
            Name = name;
        }

        public void SetValue(double value)
        {
            Value = value;
        }

        public Variable WithValue(double value)
        {
            Variable copy = new(Name);
            copy.IsConstant = IsConstant;
            copy.Value = value;
            return copy;
        }

        public void MakeConstant(double value)
        {
            IsConstant = true;
            Value = value;
        }

        public override bool Equals(object? obj)
        {
            return obj is Variable variable && ((IsConstant && variable.IsConstant && Value == variable.Value) || ReferenceEquals(this, obj));
        }

        public override string ToString()
        {
            return $"{Name}: {Value}";
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, IsConstant, Value);
        }
    }
}