using Common;
using Parser.Resolver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Parser.ParseTree
{
    public class OpChain : Expression
    {
        public override bool CanAssignTo { get { return false; } }

        public Expression Left { get; private set; }
        public Expression Right { get; private set; }
        public Token OpToken { get; private set; }
        public Ops OpTEMP { get; private set; }

        public OpChain(Expression left, Token op, Expression right, Node owner)
            : base(left.FirstToken, owner)
        {
            this.Left = left;
            this.Right = right;
            this.OpToken = op;
            this.OpTEMP = this.GetOpFromToken(op);
        }

        private Ops GetOpFromToken(Token token)
        {
            switch (token.Value)
            {
                case "+": return Ops.ADDITION;
                case "<": return Ops.LESS_THAN;
                case "<=": return Ops.LESS_THAN_OR_EQUAL;
                case ">": return Ops.GREATER_THAN;
                case ">=": return Ops.GREATER_THAN_OR_EQUAL;
                case "-": return Ops.SUBTRACTION;
                case "*": return Ops.MULTIPLICATION;
                case "/": return Ops.DIVISION;
                case "%": return Ops.MODULO;
                case "**": return Ops.EXPONENT;
                case "|": return Ops.BITWISE_OR;
                case "&": return Ops.BITWISE_AND;
                case "^": return Ops.BITWISE_XOR;
                case "<<": return Ops.BIT_SHIFT_LEFT;
                case ">>": return Ops.BIT_SHIFT_RIGHT;
                case "==": return Ops.EQUALS;
                case "!=": return Ops.NOT_EQUALS;
                default: throw new ParserException(token, "Unrecognized op: '" + token.Value + "'");
            }

        }

        public static OpChain Build(IList<Expression> expressions, IList<Token> ops, Node owner)
        {
            int expressionIndex = 0;
            int opIndex = 0;
            Expression left = expressions[expressionIndex++];
            Expression right = expressions[expressionIndex++];

            Token op = ops[opIndex++];

            OpChain boc = new OpChain(left, op, right, owner);
            while (expressionIndex < expressions.Count)
            {
                right = expressions[expressionIndex++];
                op = ops[opIndex++];
                boc = new OpChain(boc, op, right, owner);
            }

            return boc;
        }

        internal override Expression Resolve(ParserContext parser)
        {
            this.Left = this.Left.Resolve(parser);
            this.Right = this.Right.Resolve(parser);
            return this.TryConsolidate();
        }

        private static Dictionary<ResolvedTypeCategory, Dictionary<ResolvedTypeCategory, Dictionary<string, Func<OpChain, Expression>>>> consolidationLookup = null;

        private static void AddHandlerToConsolidationLookup(
            ResolvedTypeCategory leftType,
            ResolvedTypeCategory rightType,
            string op,
            Func<OpChain, Expression> handler)
        {
            if (!consolidationLookup.ContainsKey(leftType))
                consolidationLookup[leftType] = new Dictionary<ResolvedTypeCategory, Dictionary<string, Func<OpChain, Expression>>>();
            if (!consolidationLookup[leftType].ContainsKey(rightType))
                consolidationLookup[leftType][rightType] = new Dictionary<string, Func<OpChain, Expression>>();
            consolidationLookup[leftType][rightType][op] = handler;
        }

        private Expression TryConsolidate()
        {
            if (!(this.Left is IConstantValue && this.Right is IConstantValue))
            {
                return this;
            }

            ResolvedType leftType = this.Left.ResolvedType;
            ResolvedType rightType = this.Right.ResolvedType;
            ResolvedTypeCategory leftTypeCategory = leftType.Category;
            ResolvedTypeCategory rightTypeCategory = rightType.Category;

            if (leftType == ResolvedType.ANY || rightType == ResolvedType.ANY)
            {
                throw new Exception(); // This shouldn't be possible
            }

            if (consolidationLookup == null)
            {
                consolidationLookup = new Dictionary<ResolvedTypeCategory, Dictionary<ResolvedTypeCategory, Dictionary<string, Func<OpChain, Expression>>>>();
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.NULL, ResolvedTypeCategory.NULL, "==", (opChain) => { return MakeBool(opChain.FirstToken, true); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.NULL, ResolvedTypeCategory.NULL, "!=", (opChain) => { return MakeBool(opChain.FirstToken, false); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.BOOLEAN, ResolvedTypeCategory.BOOLEAN, "==", (opChain) => { return MakeBool(opChain.FirstToken, GetBool(opChain.Left) == GetBool(opChain.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.BOOLEAN, ResolvedTypeCategory.BOOLEAN, "!=", (opChain) => { return MakeBool(opChain.FirstToken, GetBool(opChain.Left) != GetBool(opChain.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.INTEGER, "&", (opChain) => { return MakeInt(opChain.FirstToken, GetInt(this.Left) & GetInt(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.INTEGER, "|", (opChain) => { return MakeInt(opChain.FirstToken, GetInt(this.Left) | GetInt(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.INTEGER, "^", (opChain) => { return MakeInt(opChain.FirstToken, GetInt(this.Left) ^ GetInt(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.INTEGER, "<<", (opChain) =>
                {
                    int right = GetInt(this.Right);
                    if (right < 0) throw new ParserException(opChain.FirstToken, "Cannot bit shift by a negative number.");
                    return MakeInt(opChain.FirstToken, GetInt(this.Left) << right);
                });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.INTEGER, ">>", (opChain) =>
                {
                    int right = GetInt(this.Right);
                    if (right < 0) throw new ParserException(opChain.FirstToken, "Cannot bit shift by a negative number.");
                    return MakeInt(opChain.FirstToken, GetInt(this.Left) >> right);
                });

                AddHandlerToConsolidationLookup(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.INTEGER, "+", (opChain) => { return MakeInt(opChain.FirstToken, GetInt(this.Left) + GetInt(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.INTEGER, "-", (opChain) => { return MakeInt(opChain.FirstToken, GetInt(this.Left) - GetInt(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.INTEGER, "*", (opChain) => { return MakeInt(opChain.FirstToken, GetInt(this.Left) * GetInt(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.INTEGER, "/", (opChain) => { CheckZero(this.Right); return MakeInt(opChain.FirstToken, GetInt(this.Left) / GetInt(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.INTEGER, "%", (opChain) => { return MakeInt(opChain.FirstToken, PositiveModInt(this.Left, this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.INTEGER, "<=", (opChain) => { return MakeBool(opChain.FirstToken, GetInt(this.Left) <= GetInt(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.INTEGER, ">=", (opChain) => { return MakeBool(opChain.FirstToken, GetInt(this.Left) >= GetInt(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.INTEGER, "<", (opChain) => { return MakeBool(opChain.FirstToken, GetInt(this.Left) < GetInt(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.INTEGER, ">", (opChain) => { return MakeBool(opChain.FirstToken, GetInt(this.Left) > GetInt(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.INTEGER, "==", (opChain) => { return MakeBool(opChain.FirstToken, GetInt(this.Left) == GetInt(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.INTEGER, "!=", (opChain) => { return MakeBool(opChain.FirstToken, GetInt(this.Left) != GetInt(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.INTEGER, "**", (opChain) =>
                {
                    int right = GetInt(this.Right);
                    int left = GetInt(this.Left);
                    if (right == 0) return MakeInt(opChain.FirstToken, 1);
                    if (right > 0) return MakeInt(opChain.FirstToken, (int)Math.Pow(left, right));
                    return MakeFloat(opChain.FirstToken, Math.Pow(left, right));
                });

                AddHandlerToConsolidationLookup(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.FLOAT, "+", (opChain) => { return MakeFloat(opChain.FirstToken, GetInt(this.Left) + GetFloat(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.FLOAT, "-", (opChain) => { return MakeFloat(opChain.FirstToken, GetInt(this.Left) - GetFloat(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.FLOAT, "*", (opChain) => { return MakeFloat(opChain.FirstToken, GetInt(this.Left) * GetFloat(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.FLOAT, "/", (opChain) => { CheckZero(this.Right); return MakeFloat(opChain.FirstToken, GetInt(this.Left) / GetFloat(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.FLOAT, "%", (opChain) => { return MakeFloat(opChain.FirstToken, PositiveModFloat(this.Left, this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.FLOAT, "<=", (opChain) => { return MakeBool(opChain.FirstToken, GetInt(this.Left) <= GetFloat(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.FLOAT, ">=", (opChain) => { return MakeBool(opChain.FirstToken, GetInt(this.Left) >= GetFloat(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.FLOAT, "<", (opChain) => { return MakeBool(opChain.FirstToken, GetInt(this.Left) < GetFloat(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.FLOAT, ">", (opChain) => { return MakeBool(opChain.FirstToken, GetInt(this.Left) > GetFloat(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.FLOAT, "==", (opChain) => { return MakeBool(opChain.FirstToken, GetInt(this.Left) == GetFloat(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.FLOAT, "!=", (opChain) => { return MakeBool(opChain.FirstToken, GetInt(this.Left) != GetFloat(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.FLOAT, "**", (opChain) =>
                {
                    double right = GetFloat(this.Right);
                    int left = GetInt(this.Left);
                    if (Util.FloatEqualsNoEpislon(right, 0)) return MakeFloat(opChain.FirstToken, 1.0);
                    return MakeFloat(opChain.FirstToken, Math.Pow(left, right));
                });

                AddHandlerToConsolidationLookup(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.INTEGER, "+", (opChain) => { return MakeFloat(opChain.FirstToken, GetFloat(this.Left) + GetInt(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.INTEGER, "-", (opChain) => { return MakeFloat(opChain.FirstToken, GetFloat(this.Left) - GetInt(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.INTEGER, "*", (opChain) => { return MakeFloat(opChain.FirstToken, GetFloat(this.Left) * GetInt(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.INTEGER, "/", (opChain) => { CheckZero(this.Right); return MakeFloat(opChain.FirstToken, GetFloat(this.Left) / GetInt(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.INTEGER, "%", (opChain) => { return MakeFloat(opChain.FirstToken, PositiveModFloat(this.Left, this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.INTEGER, "<=", (opChain) => { return MakeBool(opChain.FirstToken, GetFloat(this.Left) <= GetInt(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.INTEGER, ">=", (opChain) => { return MakeBool(opChain.FirstToken, GetFloat(this.Left) >= GetInt(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.INTEGER, "<", (opChain) => { return MakeBool(opChain.FirstToken, GetFloat(this.Left) < GetInt(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.INTEGER, ">", (opChain) => { return MakeBool(opChain.FirstToken, GetFloat(this.Left) > GetInt(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.INTEGER, "==", (opChain) => { return MakeBool(opChain.FirstToken, GetFloat(this.Left) == GetInt(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.INTEGER, "!=", (opChain) => { return MakeBool(opChain.FirstToken, GetFloat(this.Left) != GetInt(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.INTEGER, "**", (opChain) =>
                {
                    int right = GetInt(this.Right);
                    double left = GetFloat(this.Left);
                    if (right == 0) return MakeFloat(opChain.FirstToken, 1.0);
                    return MakeFloat(opChain.FirstToken, Math.Pow(left, right));
                });

                AddHandlerToConsolidationLookup(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.FLOAT, "+", (opChain) => { return MakeFloat(opChain.FirstToken, GetFloat(this.Left) + GetFloat(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.FLOAT, "-", (opChain) => { return MakeFloat(opChain.FirstToken, GetFloat(this.Left) - GetFloat(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.FLOAT, "*", (opChain) => { return MakeFloat(opChain.FirstToken, GetFloat(this.Left) * GetFloat(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.FLOAT, "/", (opChain) => { CheckZero(this.Right); return MakeFloat(opChain.FirstToken, GetFloat(this.Left) / GetFloat(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.FLOAT, "%", (opChain) => { return MakeFloat(opChain.FirstToken, PositiveModFloat(this.Left, this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.FLOAT, "<=", (opChain) => { return MakeBool(opChain.FirstToken, GetFloat(this.Left) <= GetFloat(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.FLOAT, ">=", (opChain) => { return MakeBool(opChain.FirstToken, GetFloat(this.Left) >= GetFloat(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.FLOAT, "<", (opChain) => { return MakeBool(opChain.FirstToken, GetFloat(this.Left) < GetFloat(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.FLOAT, ">", (opChain) => { return MakeBool(opChain.FirstToken, GetFloat(this.Left) > GetFloat(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.FLOAT, "==", (opChain) => { return MakeBool(opChain.FirstToken, GetFloat(this.Left) == GetFloat(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.FLOAT, "!=", (opChain) => { return MakeBool(opChain.FirstToken, GetFloat(this.Left) != GetFloat(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.FLOAT, "**", (opChain) =>
                {
                    double right = GetFloat(this.Right);
                    double left = GetFloat(this.Left);
                    if (Util.FloatEqualsNoEpislon(right, 0)) return MakeFloat(opChain.FirstToken, 1.0);
                    return MakeFloat(opChain.FirstToken, Math.Pow(left, right));
                });

                AddHandlerToConsolidationLookup(ResolvedTypeCategory.BOOLEAN, ResolvedTypeCategory.STRING, "+", (opChain) => { return MakeString(opChain.FirstToken, GetBool(this.Left).ToString() + GetString(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.STRING, "+", (opChain) => { return MakeString(opChain.FirstToken, GetInt(this.Left).ToString() + GetString(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.STRING, "+", (opChain) => { return MakeString(opChain.FirstToken, GetFloatAsString(this.Left) + GetString(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.STRING, ResolvedTypeCategory.BOOLEAN, "+", (opChain) => { return MakeString(opChain.FirstToken, GetString(this.Left) + GetBool(this.Right).ToString()); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.STRING, ResolvedTypeCategory.INTEGER, "+", (opChain) => { return MakeString(opChain.FirstToken, GetString(this.Left) + GetInt(this.Right).ToString()); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.STRING, ResolvedTypeCategory.FLOAT, "+", (opChain) => { return MakeString(opChain.FirstToken, GetString(this.Left) + GetFloatAsString(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.STRING, ResolvedTypeCategory.STRING, "+", (opChain) => { return MakeString(opChain.FirstToken, GetString(this.Left) + GetString(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.STRING, ResolvedTypeCategory.STRING, "==", (opChain) => { return MakeBool(opChain.FirstToken, GetString(this.Left) == GetString(this.Right)); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.STRING, ResolvedTypeCategory.STRING, "!=", (opChain) => { return MakeBool(opChain.FirstToken, GetString(this.Left) != GetString(this.Right)); });

                AddHandlerToConsolidationLookup(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.STRING, "*", (opChain) => { return GenerateMultipliedStringIfNotTooLong(opChain, opChain.Left, opChain.Right); });
                AddHandlerToConsolidationLookup(ResolvedTypeCategory.STRING, ResolvedTypeCategory.INTEGER, "*", (opChain) => { return GenerateMultipliedStringIfNotTooLong(opChain, opChain.Left, opChain.Right); });
            }

            if (consolidationLookup.ContainsKey(leftTypeCategory) &&
                consolidationLookup[leftTypeCategory].ContainsKey(rightTypeCategory) &&
                consolidationLookup[leftTypeCategory][rightTypeCategory].ContainsKey(this.OpToken.Value))
            {
                return consolidationLookup[leftTypeCategory][rightTypeCategory][this.OpToken.Value].Invoke(this);
            }

            throw new ParserException(this.OpToken, "This operator is invalid for types: " + leftType + ", " + rightType + ".");
        }

        private static Expression GenerateMultipliedStringIfNotTooLong(OpChain original, Expression left, Expression right)
        {
            string stringValue = ((left as StringConstant) ?? (right as StringConstant)).Value;
            int intValue = ((left as IntegerConstant) ?? (right as IntegerConstant)).Value;

            // don't consolidate this operation if it's going to make the file size blow up.
            if (intValue * stringValue.Length <= 50)
            {
                string output = "";
                while (intValue > 0)
                {
                    output += stringValue;
                    --intValue;
                }
                return new StringConstant(original.FirstToken, output, original.Owner);
            }

            return original;
        }

        private int PositiveModInt(Expression left, Expression right)
        {
            CheckZero(right);
            int leftInt = GetInt(left);
            int rightInt = GetInt(right);
            int value = leftInt % rightInt;
            if (value < 0) value += rightInt;
            return value;
        }

        private double PositiveModFloat(Expression left, Expression right)
        {
            CheckZero(right);
            double leftValue = left is FloatConstant
                ? ((FloatConstant)left).Value
                : (0.0 + ((IntegerConstant)left).Value);
            double rightValue = right is FloatConstant
                ? ((FloatConstant)right).Value
                : (0.0 + ((IntegerConstant)right).Value);
            double value = leftValue % rightValue;
            if (value < 0) value += rightValue;
            return value;
        }

        private void CheckZero(Expression expr)
        {
            bool isZero = false;
            if (expr is IntegerConstant)
            {
                isZero = ((IntegerConstant)expr).Value == 0;
            }
            else
            {
                isZero = Util.FloatEqualsNoEpislon(((FloatConstant)expr).Value, 0);
            }
            if (isZero)
            {
                throw new ParserException(expr, "Division by 0 error.");
            }
        }

        private Expression MakeFloat(Token firstToken, double value)
        {
            return new FloatConstant(firstToken, value, this.Owner);
        }

        private Expression MakeInt(Token firstToken, int value)
        {
            return new IntegerConstant(firstToken, value, this.Owner);
        }

        private Expression MakeString(Token firstToken, string value)
        {
            return new StringConstant(firstToken, value, this.Owner);
        }

        private bool GetBool(Expression expr)
        {
            return ((BooleanConstant)expr).Value;
        }

        private int GetInt(Expression expr)
        {
            return ((IntegerConstant)expr).Value;
        }

        private double GetFloat(Expression expr)
        {
            return ((FloatConstant)expr).Value;
        }

        private string GetFloatAsString(Expression expr)
        {
            string value = ((FloatConstant)expr).Value.ToString();
            if (!value.Contains('.'))
            {
                value += ".0";
            }
            return value;
        }

        private string GetString(Expression expr)
        {
            return ((StringConstant)expr).Value;
        }

        private Expression MakeBool(Token firstToken, bool value)
        {
            return new BooleanConstant(firstToken, value, this.Owner);
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase)
        {
            if ((phase & VariableIdAllocPhase.ALLOC) != 0)
            {
                this.Left.PerformLocalIdAllocation(parser, varIds, phase);
                this.Right.PerformLocalIdAllocation(parser, varIds, phase);
            }
        }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            this.Left = this.Left.ResolveEntityNames(parser);
            this.Right = this.Right.ResolveEntityNames(parser);
            return this;
        }

        internal override void ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            this.Left.ResolveTypes(parser, typeResolver);
            this.Right.ResolveTypes(parser, typeResolver);

            throw new NotImplementedException();
        }
    }
}
