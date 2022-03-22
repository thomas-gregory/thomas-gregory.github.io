using Parser.Eval;
using Parser.Eval.Functions;
using Parser.Eval.Expressions;

Scope scope = new();
Variable x = scope.CreateVariable("x");
scope.CreateFunction("sin", new UnaryFunction(Math.Sin, true));
scope.CreateFunction("coth", new UnaryFunction(y => 1 / Math.Tanh(y), true));
scope.CreateFunction("ln", new UnaryFunction(Math.Log, true));
Expression result = Parser.Parse(File.ReadAllText("test.txt"), scope);
x.SetValue(0.1);
Console.WriteLine(result.Evaluate());