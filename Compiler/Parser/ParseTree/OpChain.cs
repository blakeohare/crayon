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

        private static Dictionary<string, Dictionary<ResolvedTypeCategory, Dictionary<ResolvedTypeCategory, OperationType>>> consolidationLookup = null;

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
                // This shouldn't be possible if it's already confirmed that the left and right inputs are constants.
                throw new Exception();
            }

            OperationType ot = GetOperation(leftTypeCategory, rightTypeCategory, this.OpToken.Value);
            if (ot != null && ot.CanDoConstantOperationAtRuntime)
            {
                return ot.PerformOperation.Invoke(this);
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

            if (this.Left.ResolvedType == ResolvedType.ANY || this.Right.ResolvedType == ResolvedType.ANY)
            {
                if (this.Left.ResolvedType == ResolvedType.STRING || this.Right.ResolvedType == ResolvedType.STRING)
                {
                    this.ResolvedType = ResolvedType.STRING;
                }
                else
                {
                    this.ResolvedType = ResolvedType.ANY;
                }
            }
            else
            {
                OperationType ot = GetOperation(this.Left.ResolvedType.Category, this.Right.ResolvedType.Category, this.OpToken.Value);
                if (ot == null)
                {
                    throw new ParserException(this.OpToken, "This operation is not allowed between these two types.");
                }

                this.ResolvedType = ot.OutputType;
            }
        }

        private OperationType GetOperation(
            ResolvedTypeCategory leftType,
            ResolvedTypeCategory rightType,
            string op)
        {
            if (consolidationLookup == null)
            {
                consolidationLookup = new Dictionary<string, Dictionary<ResolvedTypeCategory, Dictionary<ResolvedTypeCategory, OperationType>>>();

                OperationType[] operations = new OperationType[] {
                    new OperationType(ResolvedTypeCategory.NULL, ResolvedTypeCategory.NULL, "==", ResolvedType.BOOLEAN, (opChain) => { return MakeBool(opChain.FirstToken, true); }),
                    new OperationType(ResolvedTypeCategory.NULL, ResolvedTypeCategory.NULL, "!=", ResolvedType.BOOLEAN, (opChain) => { return MakeBool(opChain.FirstToken, false); }),
                    new OperationType(ResolvedTypeCategory.BOOLEAN, ResolvedTypeCategory.BOOLEAN, "==", ResolvedType.BOOLEAN, (opChain) => { return MakeBool(opChain.FirstToken, GetBool(opChain.Left) == GetBool(opChain.Right)); }),
                    new OperationType(ResolvedTypeCategory.BOOLEAN, ResolvedTypeCategory.BOOLEAN, "!=", ResolvedType.BOOLEAN, (opChain) => { return MakeBool(opChain.FirstToken, GetBool(opChain.Left) != GetBool(opChain.Right)); }),
                    new OperationType(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.INTEGER, "&", ResolvedType.INTEGER, (opChain) => { return MakeInt(opChain.FirstToken, GetInt(this.Left) & GetInt(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.INTEGER, "|", ResolvedType.INTEGER, (opChain) => { return MakeInt(opChain.FirstToken, GetInt(this.Left) | GetInt(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.INTEGER,  "^", ResolvedType.INTEGER, (opChain) => { return MakeInt(opChain.FirstToken, GetInt(this.Left) ^ GetInt(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.INTEGER, "<<", ResolvedType.INTEGER, (opChain) =>
                    {
                        int right = GetInt(this.Right);
                        if (right < 0) throw new ParserException(opChain.FirstToken, "Cannot bit shift by a negative number.");
                        return MakeInt(opChain.FirstToken, GetInt(this.Left) << right);
                    }),
                    new OperationType(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.INTEGER, ">>", ResolvedType.INTEGER, (opChain) =>
                    {
                        int right = GetInt(this.Right);
                        if (right < 0) throw new ParserException(opChain.FirstToken, "Cannot bit shift by a negative number.");
                        return MakeInt(opChain.FirstToken, GetInt(this.Left) >> right);
                    }),

                    new OperationType(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.INTEGER, "+", ResolvedType.INTEGER, (opChain) => { return MakeInt(opChain.FirstToken, GetInt(this.Left) + GetInt(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.INTEGER, "-", ResolvedType.INTEGER, (opChain) => { return MakeInt(opChain.FirstToken, GetInt(this.Left) - GetInt(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.INTEGER, "*", ResolvedType.INTEGER, (opChain) => { return MakeInt(opChain.FirstToken, GetInt(this.Left) * GetInt(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.INTEGER, "/", ResolvedType.INTEGER, (opChain) => { CheckZero(this.Right); return MakeInt(opChain.FirstToken, GetInt(this.Left) / GetInt(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.INTEGER, "%", ResolvedType.INTEGER, (opChain) => { return MakeInt(opChain.FirstToken, PositiveModInt(this.Left, this.Right)); }),
                    new OperationType(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.INTEGER, "<=", ResolvedType.BOOLEAN, (opChain) => { return MakeBool(opChain.FirstToken, GetInt(this.Left) <= GetInt(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.INTEGER, ">=", ResolvedType.BOOLEAN, (opChain) => { return MakeBool(opChain.FirstToken, GetInt(this.Left) >= GetInt(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.INTEGER, "<", ResolvedType.BOOLEAN, (opChain) => { return MakeBool(opChain.FirstToken, GetInt(this.Left) < GetInt(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.INTEGER, ">", ResolvedType.BOOLEAN, (opChain) => { return MakeBool(opChain.FirstToken, GetInt(this.Left) > GetInt(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.INTEGER, "==", ResolvedType.BOOLEAN, (opChain) => { return MakeBool(opChain.FirstToken, GetInt(this.Left) == GetInt(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.INTEGER, "!=", ResolvedType.BOOLEAN, (opChain) => { return MakeBool(opChain.FirstToken, GetInt(this.Left) != GetInt(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.INTEGER, "**", ResolvedType.FLOAT, (opChain) =>
                    {
                        int right = GetInt(this.Right);
                        int left = GetInt(this.Left);
                        if (right == 0) return MakeInt(opChain.FirstToken, 1);
                        return MakeFloat(opChain.FirstToken, Math.Pow(left, right));
                    }),

                    new OperationType(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.FLOAT, "+", ResolvedType.FLOAT, (opChain) => { return MakeFloat(opChain.FirstToken, GetInt(this.Left) + GetFloat(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.FLOAT, "-", ResolvedType.FLOAT, (opChain) => { return MakeFloat(opChain.FirstToken, GetInt(this.Left) - GetFloat(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.FLOAT, "*", ResolvedType.FLOAT, (opChain) => { return MakeFloat(opChain.FirstToken, GetInt(this.Left) * GetFloat(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.FLOAT, "/", ResolvedType.FLOAT, (opChain) => { CheckZero(this.Right); return MakeFloat(opChain.FirstToken, GetInt(this.Left) / GetFloat(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.FLOAT, "%", ResolvedType.FLOAT, (opChain) => { return MakeFloat(opChain.FirstToken, PositiveModFloat(this.Left, this.Right)); }),
                    new OperationType(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.FLOAT, "<=", ResolvedType.BOOLEAN, (opChain) => { return MakeBool(opChain.FirstToken, GetInt(this.Left) <= GetFloat(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.FLOAT, ">=", ResolvedType.BOOLEAN, (opChain) => { return MakeBool(opChain.FirstToken, GetInt(this.Left) >= GetFloat(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.FLOAT, "<", ResolvedType.BOOLEAN, (opChain) => { return MakeBool(opChain.FirstToken, GetInt(this.Left) < GetFloat(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.FLOAT, ">", ResolvedType.BOOLEAN, (opChain) => { return MakeBool(opChain.FirstToken, GetInt(this.Left) > GetFloat(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.FLOAT, "==", ResolvedType.BOOLEAN, (opChain) => { return MakeBool(opChain.FirstToken, GetInt(this.Left) == GetFloat(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.FLOAT, "!=", ResolvedType.BOOLEAN, (opChain) => { return MakeBool(opChain.FirstToken, GetInt(this.Left) != GetFloat(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.FLOAT, "**", ResolvedType.FLOAT, (opChain) =>
                    {
                        double right = GetFloat(this.Right);
                        int left = GetInt(this.Left);
                        if (Util.FloatEqualsNoEpislon(right, 0)) return MakeFloat(opChain.FirstToken, 1.0);
                        return MakeFloat(opChain.FirstToken, Math.Pow(left, right));
                    }),

                    new OperationType(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.INTEGER, "+", ResolvedType.FLOAT, (opChain) => { return MakeFloat(opChain.FirstToken, GetFloat(this.Left) + GetInt(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.INTEGER, "-", ResolvedType.FLOAT, (opChain) => { return MakeFloat(opChain.FirstToken, GetFloat(this.Left) - GetInt(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.INTEGER, "*", ResolvedType.FLOAT, (opChain) => { return MakeFloat(opChain.FirstToken, GetFloat(this.Left) * GetInt(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.INTEGER, "/", ResolvedType.FLOAT, (opChain) => { CheckZero(this.Right); return MakeFloat(opChain.FirstToken, GetFloat(this.Left) / GetInt(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.INTEGER, "%", ResolvedType.FLOAT, (opChain) => { return MakeFloat(opChain.FirstToken, PositiveModFloat(this.Left, this.Right)); }),
                    new OperationType(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.INTEGER, "<=", ResolvedType.BOOLEAN, (opChain) => { return MakeBool(opChain.FirstToken, GetFloat(this.Left) <= GetInt(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.INTEGER, ">=", ResolvedType.BOOLEAN, (opChain) => { return MakeBool(opChain.FirstToken, GetFloat(this.Left) >= GetInt(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.INTEGER, "<", ResolvedType.BOOLEAN, (opChain) => { return MakeBool(opChain.FirstToken, GetFloat(this.Left) < GetInt(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.INTEGER, ">", ResolvedType.BOOLEAN, (opChain) => { return MakeBool(opChain.FirstToken, GetFloat(this.Left) > GetInt(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.INTEGER, "==", ResolvedType.BOOLEAN, (opChain) => { return MakeBool(opChain.FirstToken, GetFloat(this.Left) == GetInt(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.INTEGER, "!=", ResolvedType.BOOLEAN, (opChain) => { return MakeBool(opChain.FirstToken, GetFloat(this.Left) != GetInt(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.INTEGER, "**", ResolvedType.FLOAT, (opChain) =>
                    {
                        int right = GetInt(this.Right);
                        double left = GetFloat(this.Left);
                        if (right == 0) return MakeFloat(opChain.FirstToken, 1.0);
                        return MakeFloat(opChain.FirstToken, Math.Pow(left, right));
                    }),

                    new OperationType(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.FLOAT, "+", ResolvedType.FLOAT, (opChain) => { return MakeFloat(opChain.FirstToken, GetFloat(this.Left) + GetFloat(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.FLOAT, "-", ResolvedType.FLOAT, (opChain) => { return MakeFloat(opChain.FirstToken, GetFloat(this.Left) - GetFloat(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.FLOAT, "*", ResolvedType.FLOAT, (opChain) => { return MakeFloat(opChain.FirstToken, GetFloat(this.Left) * GetFloat(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.FLOAT, "/", ResolvedType.FLOAT, (opChain) => { CheckZero(this.Right); return MakeFloat(opChain.FirstToken, GetFloat(this.Left) / GetFloat(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.FLOAT, "%", ResolvedType.FLOAT, (opChain) => { return MakeFloat(opChain.FirstToken, PositiveModFloat(this.Left, this.Right)); }),
                    new OperationType(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.FLOAT, "<=", ResolvedType.BOOLEAN, (opChain) => { return MakeBool(opChain.FirstToken, GetFloat(this.Left) <= GetFloat(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.FLOAT, ">=", ResolvedType.BOOLEAN, (opChain) => { return MakeBool(opChain.FirstToken, GetFloat(this.Left) >= GetFloat(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.FLOAT, "<", ResolvedType.BOOLEAN, (opChain) => { return MakeBool(opChain.FirstToken, GetFloat(this.Left) < GetFloat(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.FLOAT, ">", ResolvedType.BOOLEAN, (opChain) => { return MakeBool(opChain.FirstToken, GetFloat(this.Left) > GetFloat(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.FLOAT, "==", ResolvedType.BOOLEAN, (opChain) => { return MakeBool(opChain.FirstToken, GetFloat(this.Left) == GetFloat(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.FLOAT, "!=", ResolvedType.BOOLEAN, (opChain) => { return MakeBool(opChain.FirstToken, GetFloat(this.Left) != GetFloat(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.FLOAT, "**", ResolvedType.FLOAT, (opChain) =>
                    {
                        double right = GetFloat(this.Right);
                        double left = GetFloat(this.Left);
                        if (Util.FloatEqualsNoEpislon(right, 0)) return MakeFloat(opChain.FirstToken, 1.0);
                        return MakeFloat(opChain.FirstToken, Math.Pow(left, right));
                    }),

                    new OperationType(ResolvedTypeCategory.BOOLEAN, ResolvedTypeCategory.STRING, "+", ResolvedType.STRING, (opChain) => { return MakeString(opChain.FirstToken, GetBool(this.Left).ToString() + GetString(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.STRING, "+", ResolvedType.STRING, (opChain) => { return MakeString(opChain.FirstToken, GetInt(this.Left).ToString() + GetString(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.FLOAT, ResolvedTypeCategory.STRING, "+", ResolvedType.STRING, (opChain) => { return MakeString(opChain.FirstToken, GetFloatAsString(this.Left) + GetString(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.STRING, ResolvedTypeCategory.BOOLEAN, "+", ResolvedType.STRING, (opChain) => { return MakeString(opChain.FirstToken, GetString(this.Left) + GetBool(this.Right).ToString()); }),
                    new OperationType(ResolvedTypeCategory.STRING, ResolvedTypeCategory.INTEGER, "+", ResolvedType.STRING, (opChain) => { return MakeString(opChain.FirstToken, GetString(this.Left) + GetInt(this.Right).ToString()); }),
                    new OperationType(ResolvedTypeCategory.STRING, ResolvedTypeCategory.FLOAT, "+", ResolvedType.STRING, (opChain) => { return MakeString(opChain.FirstToken, GetString(this.Left) + GetFloatAsString(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.STRING, ResolvedTypeCategory.STRING, "+", ResolvedType.STRING, (opChain) => { return MakeString(opChain.FirstToken, GetString(this.Left) + GetString(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.STRING, ResolvedTypeCategory.STRING, "==", ResolvedType.BOOLEAN, (opChain) => { return MakeBool(opChain.FirstToken, GetString(this.Left) == GetString(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.STRING, ResolvedTypeCategory.STRING, "!=", ResolvedType.BOOLEAN, (opChain) => { return MakeBool(opChain.FirstToken, GetString(this.Left) != GetString(this.Right)); }),

                    new OperationType(ResolvedTypeCategory.BOOLEAN, ResolvedTypeCategory.BOOLEAN, "&&", ResolvedType.BOOLEAN, (opChain) => { return MakeBool(opChain.FirstToken, GetBool(this.Left) && GetBool(this.Right)); }),
                    new OperationType(ResolvedTypeCategory.BOOLEAN, ResolvedTypeCategory.BOOLEAN, "||", ResolvedType.BOOLEAN, (opChain) => { return MakeBool(opChain.FirstToken, GetBool(this.Left) || GetBool(this.Right)); }),

                    new OperationType(ResolvedTypeCategory.INTEGER, ResolvedTypeCategory.STRING, "*", ResolvedType.STRING, (opChain) => { return GenerateMultipliedStringIfNotTooLong(opChain, opChain.Left, opChain.Right); }),
                    new OperationType(ResolvedTypeCategory.STRING, ResolvedTypeCategory.INTEGER, "*", ResolvedType.STRING, (opChain) => { return GenerateMultipliedStringIfNotTooLong(opChain, opChain.Left, opChain.Right); }),
                };

                foreach (OperationType ot in operations)
                {
                    if (!consolidationLookup.ContainsKey(ot.Op))
                        consolidationLookup[ot.Op] = new Dictionary<ResolvedTypeCategory, Dictionary<ResolvedTypeCategory, OperationType>>();
                    if (!consolidationLookup[ot.Op].ContainsKey(ot.LeftType))
                        consolidationLookup[ot.Op][ot.LeftType] = new Dictionary<ResolvedTypeCategory, OperationType>();
                    consolidationLookup[ot.Op][ot.LeftType].Add(ot.RightType, ot); // causes exception if duplicate
                }
            }

            // The op will always have some types registered, so you can dereference the first level of lookups without checking.
            Dictionary<ResolvedTypeCategory, Dictionary<ResolvedTypeCategory, OperationType>> l1 = consolidationLookup[op];
            if (!l1.ContainsKey(leftType)) return null;
            Dictionary<ResolvedTypeCategory, OperationType> l2 = l1[leftType];
            if (!l2.ContainsKey(rightType)) return null;
            return l2[rightType];
        }

        private class OperationType
        {
            public OperationType(
                ResolvedTypeCategory leftType,
                ResolvedTypeCategory rightType,
                string op,
                ResolvedType outputType,
                Func<OpChain, Expression> constantOperation)
            {
                this.LeftType = leftType;
                this.Op = op;
                this.RightType = rightType;
                this.OutputType = outputType;
                this.PerformOperation = constantOperation;
            }

            public ResolvedTypeCategory LeftType { get; private set; }
            public ResolvedTypeCategory RightType { get; private set; }
            public string Op { get; private set; }
            public ResolvedType OutputType { get; private set; }

            public bool CanDoConstantOperationAtRuntime { get { return this.PerformOperation != null; } }
            public Func<OpChain, Expression> PerformOperation { get; set; }
        }
    }
}
