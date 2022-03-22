using Parser.Tokenizers;
using Parser.Tokenizers.Position;
using Parser.Eval.Functions;
using Parser.Eval.Expressions;

namespace Parser.Eval
{
    public class Parser
    {
        private readonly Scope Scope;
        private readonly Tokenizer Tokenizer;

        static Parser() { }

        protected Parser(Stream input, Scope scope)
        {
            Scope = scope;
            Scope.CreateConstant("tau", Math.Tau);
            Scope.CreateConstant("e", Math.E);
            Tokenizer = new Tokenizer(input);
        }

        public static Expression Parse(string input, Scope? scope = null)
            => new Parser(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(input)), scope ?? new Scope()).Parse();

        public static Expression Parse(Stream input, Scope? scope = null)
            => new Parser(input, scope ?? new Scope()).Parse();

        protected Expression Parse()
        {
            Expression result = Expression().Simplify();
            if (!Tokenizer.Current.IsEnd)
            {
                Token token = Tokenizer.Consume();
                throw new ArgumentException($"Unexpected token: '{token.Source}'. Expected an expression.");
            }

            return result;
        }

        protected Expression Expression()
        {
            Expression condition = BooleanExpression();
            if (Tokenizer.Current.IsSymbol("?"))
            {
                Tokenizer.Consume();
                Expression left = Expression();
                if (Tokenizer.Current.IsSymbol(":"))
                {
                    Tokenizer.Consume();
                    Expression right = Expression();

                    FunctionCall result = new(
                        new GeneralFunction(
                            arguments =>
                            {
                                if (Math.Abs(arguments[0].Evaluate()) > double.Epsilon)
                                {
                                    return arguments[1].Evaluate();
                                }

                                return arguments[2].Evaluate();
                            }
                        ),
                        new List<Expression>
                        {
                            condition,
                            left,
                            right,
                        }
                    );
                    return result;
                }
                throw new ArgumentException("Expected ':'");
            }

            return condition;
        }

        protected Expression BooleanExpression()
        {
            Expression left = RelationalExpression();
            if (Tokenizer.Current.IsSymbol("&&"))
            {
                Tokenizer.Consume();
                Expression right = BooleanExpression();
                return ReOrder(left, right, BinaryOperation.Operations.And);
            }
            if (Tokenizer.Current.IsSymbol("||"))
            {
                Tokenizer.Consume();
                Expression right = BooleanExpression();
                return ReOrder(left, right, BinaryOperation.Operations.Or);
            }

            return left;
        }

        protected Expression RelationalExpression()
        {
            Expression left = Term();
            Expression right;
            if (Tokenizer.Current.IsSymbol("<"))
            {
                Tokenizer.Consume();
                right = RelationalExpression();
                return ReOrder(left, right, BinaryOperation.Operations.LessThan);
            }
            if (Tokenizer.Current.IsSymbol("<="))
            {
                Tokenizer.Consume();
                right = RelationalExpression();
                return ReOrder(left, right, BinaryOperation.Operations.LessThanOrEqual);
            }
            if (Tokenizer.Current.IsSymbol(">"))
            {
                Tokenizer.Consume();
                right = RelationalExpression();
                return ReOrder(left, right, BinaryOperation.Operations.GreaterThan);
            }
            if (Tokenizer.Current.IsSymbol(">="))
            {
                Tokenizer.Consume();
                right = RelationalExpression();
                return ReOrder(left, right, BinaryOperation.Operations.GreaterThanOrEqual);
            }
            if (Tokenizer.Current.IsSymbol("=="))
            {
                Tokenizer.Consume();
                right = RelationalExpression();
                return ReOrder(left, right, BinaryOperation.Operations.Equal);
            }
            if (Tokenizer.Current.IsSymbol("!="))
            {
                Tokenizer.Consume();
                right = RelationalExpression();
                return ReOrder(left, right, BinaryOperation.Operations.NotEqual);
            }
            return left;
        }

        public Expression Term()
        {
            Expression left = Product();
            if (Tokenizer.Current.IsSymbol("+"))
            {
                Tokenizer.Consume();
                Expression right = Term();
                return ReOrder(left, right, BinaryOperation.Operations.Add);
            }
            if (Tokenizer.Current.IsSymbol("-"))
            {
                Tokenizer.Consume();
                Expression right = Term();
                return ReOrder(left, right, BinaryOperation.Operations.Subtract);
            }
            if (Tokenizer.Current.IsNumber)
            {
                if (Tokenizer.Current.Contents.StartsWith("-"))
                {
                    Expression right = Term();
                    return ReOrder(left, right, BinaryOperation.Operations.Add);
                }
            }

            return left;
        }

        public Expression Product()
        {
            Expression left = Power();
            if (Tokenizer.Current.IsSymbol("*"))
            {
                Tokenizer.Consume();
                Expression right = Product();
                return ReOrder(left, right, BinaryOperation.Operations.Multiply);
            }
            if (Tokenizer.Current.IsSymbol("/"))
            {
                Tokenizer.Consume();
                Expression right = Product();
                return ReOrder(left, right, BinaryOperation.Operations.Divide);
            }
            if (Tokenizer.Current.IsSymbol("%"))
            {
                Tokenizer.Consume();
                Expression right = Product();
                return ReOrder(left, right, BinaryOperation.Operations.Modulo);
            }
            return left;
        }

        protected Expression ReOrder(Expression left, Expression right, BinaryOperation.Operations op)
        {
            if (right is BinaryOperation rightOp && !rightOp.IsSealed && BinaryOperation.GetPriority(rightOp.Operation) == BinaryOperation.GetPriority(op))
            {
                ReplaceLeft(rightOp, left, op);
                return right;
            }
            return new BinaryOperation(op, left, right);
        }

        protected void ReplaceLeft(BinaryOperation target, Expression newLeft, BinaryOperation.Operations op)
        {
            if (target.Left is BinaryOperation leftOp && !leftOp.IsSealed && BinaryOperation.GetPriority(leftOp.Operation) == BinaryOperation.GetPriority(op))
            {
                ReplaceLeft(leftOp, newLeft, op);
                return;
            }
            target.Left = new BinaryOperation(op, newLeft, target.Left);
        }

        protected Expression Power()
        {
            Expression left = Atom();
            if (Tokenizer.Current.IsSymbol("^") || Tokenizer.Current.IsSymbol("**"))
            {
                Tokenizer.Consume();
                Expression right = Power();
                return ReOrder(left, right, BinaryOperation.Operations.Power);
            }
            return left;
        }

        protected Expression Atom()
        {
            if (Tokenizer.Current.IsSymbol("-"))
            {
                Tokenizer.Consume();
                BinaryOperation result = new(BinaryOperation.Operations.Subtract, new Constant(0), Atom());
                result.Seal();
                return result;
            }

            if (Tokenizer.Current.IsSymbol("+") && Tokenizer.Next.IsSymbol("("))
            {
                Tokenizer.Consume();
            }

            if (Tokenizer.Current.IsSymbol("("))
            {
                Tokenizer.Consume();
                Expression result = Expression();

                if (result is BinaryOperation resultOp) { resultOp.Seal(); }
                Expect(Token.TokenType.Symbol, ")");
                return result;
            }

            if (Tokenizer.Current.IsSymbol("|"))
            {
                Tokenizer.Consume();
                FunctionCall call = new(new UnaryFunction(Math.Abs, true), new List<Expression> { Expression() });

                Expect(Token.TokenType.Symbol, "|");
                return call;
            }

            if (Tokenizer.Current.IsIdentifier())
            {
                if (Tokenizer.Next.IsSymbol("("))
                {
                    return FunctionCall();
                }

                Token variableName = Tokenizer.Consume();
                Variable? referencedVariable = Scope.GetVariable(variableName.Contents);

                if (referencedVariable is not null)
                {
                    return new VariableReference(referencedVariable);
                }
                
                throw new ArgumentException($"Unknown variable: '{variableName}'");
            }
            return LiteralAtom();
        }

        private Expression LiteralAtom()
        {
            Token token;
            if (Tokenizer.Current.IsSymbol("+") && Tokenizer.Next.IsNumber) { Tokenizer.Consume(); }
            if (Tokenizer.Current.IsNumber)
            {
                double value = double.Parse(Tokenizer.Consume().Contents);
                if (Tokenizer.Current.Is(Token.TokenType.ID))
                {
                    string quantifier = string.Intern(Tokenizer.Current.Contents);
                    switch (quantifier)
                    {
                        case "n":
                            value /= 1000000000D;
                            Tokenizer.Consume();
                            break;
                        case "u":
                            value /= 1000000D;
                            Tokenizer.Consume();
                            break;
                        case "m":
                            value /= 1000D;
                            Tokenizer.Consume();
                            break;
                        case "K" or "k":
                            value *= 1000D;
                            Tokenizer.Consume();
                            break;
                        case "M":
                            value *= 1000000D;
                            Tokenizer.Consume();
                            break;
                        case "G":
                            value *= 1000000000D;
                            Tokenizer.Consume();
                            break;
                        default:
                            token = Tokenizer.Consume();
                            throw new ArgumentException($"Unexpected token: '{token.Source}'. Expected a valid quantifier.");
                    }
                }
                return new Constant(value);
            }
            token = Tokenizer.Consume();
            throw new ArgumentException($"Unexpected token: '{token.Source}'. Expected an expression.");
        }

        protected Expression FunctionCall()
        {
            Token funcToken = Tokenizer.Consume();
            Function? referencedFunction = Scope.GetFunction(funcToken.Contents);
            if (referencedFunction is null)
            {
                throw new ArgumentException($"Unknown function: '{funcToken.Contents}'");
            }

            List<Expression> callArguments = new();
            Tokenizer.Consume();
            while (!Tokenizer.Current.IsSymbol(")") && !Tokenizer.Current.IsEnd)
            {
                if (callArguments.Count != 0) { Expect(Token.TokenType.Symbol, ","); }
                callArguments.Add(Expression());
            }

            FunctionCall call = new(referencedFunction, callArguments);

            Expect(Token.TokenType.Symbol, ")");
            return call;
        }

        protected void Expect(Token.TokenType type, string trigger)
        {
            if (Tokenizer.Current.Matches(type, trigger)) { Tokenizer.Consume(); }
            else
            {
                throw new System.ArgumentException($"Unexpected token: '{Tokenizer.Current.Source}'. Expected: '{trigger}'.");
            }
        }
    }
}