using System;
using System.Collections.Generic;
using System.Linq;

namespace Parser.ParseTree
{
    public class BinaryOpChain : Expression
    {
        internal override Expression PastelResolve(ParserContext parser)
        {
            this.Left = this.Left.PastelResolve(parser);
            this.Right = this.Right.PastelResolve(parser);
            return this;
        }

        public override bool CanAssignTo { get { return false; } }

        public Expression Left { get; private set; }
        public Expression Right { get; private set; }
        public Token Op { get; private set; }

        public BinaryOpChain(Expression left, Token op, Expression right, TopLevelConstruct owner)
            : base(left.FirstToken, owner)
        {
            this.Left = left;
            this.Right = right;
            this.Op = op;
        }

        public static BinaryOpChain Build(IList<Expression> expressions, IList<Token> ops, TopLevelConstruct owner)
        {
            int expressionIndex = 0;
            int opIndex = 0;
            Expression left = expressions[expressionIndex++];
            Expression right = expressions[expressionIndex++];

            Token op = ops[opIndex++];

            BinaryOpChain boc = new BinaryOpChain(left, op, right, owner);
            while (expressionIndex < expressions.Count)
            {
                right = expressions[expressionIndex++];
                op = ops[opIndex++];
                boc = new BinaryOpChain(boc, op, right, owner);
            }

            return boc;
        }

        private string GetType(Expression expr)
        {
            if (expr is IntegerConstant) return "int";
            if (expr is FloatConstant) return "float";
            if (expr is BooleanConstant) return "bool";
            if (expr is NullConstant) return "null";
            if (expr is StringConstant) return "string";
            return "other";
        }

        internal override Expression Resolve(ParserContext parser)
        {
            this.Left = this.Left.Resolve(parser);
            this.Right = this.Right.Resolve(parser);
            string leftType = this.GetType(this.Left);
            string rightType = this.GetType(this.Right);
            return this.TryConsolidate(this.Left.FirstToken, leftType, this.Op, rightType, this.Left, this.Right);
        }

        private Expression TryConsolidate(Token firstToken, string leftType, Token opToken, string rightType, Expression left, Expression right)
        {
            // eliminate all non constants
            if (leftType == "other" || rightType == "other") return this;
            Token tk = firstToken;

            int leftInt, rightInt;
            double leftFloat, rightFloat;

            switch (leftType + " " + opToken.Value + " " + rightType)
            {
                case "null == null": return MakeBool(tk, true);
                case "null != null": return MakeBool(tk, false);

                case "bool == bool": return MakeBool(tk, GetBool(left) == GetBool(right));
                case "bool != bool": return MakeBool(tk, GetBool(left) != GetBool(right));

                case "int + int": return MakeInt(tk, GetInt(left) + GetInt(right));
                case "int - int": return MakeInt(tk, GetInt(left) - GetInt(right));
                case "int * int": return MakeInt(tk, GetInt(left) * GetInt(right));
                case "int / int": CheckZero(right); return MakeInt(tk, GetInt(left) / GetInt(right));
                case "int ** int":
                    rightInt = GetInt(right);
                    leftInt = GetInt(left);
                    if (rightInt == 0) return MakeInt(tk, 1);
                    if (rightInt > 0) return MakeInt(tk, (int)Math.Pow(leftInt, rightInt));
                    return MakeFloat(tk, Math.Pow(leftInt, rightInt));
                case "int % int": return MakeInt(tk, PositiveModInt(left, right));
                case "int & int": return MakeInt(tk, GetInt(left) & GetInt(right));
                case "int | int": return MakeInt(tk, GetInt(left) | GetInt(right));
                case "int ^ int": return MakeInt(tk, GetInt(left) ^ GetInt(right));
                case "int <= int": return MakeBool(tk, GetInt(left) <= GetInt(right));
                case "int >= int": return MakeBool(tk, GetInt(left) >= GetInt(right));
                case "int < int": return MakeBool(tk, GetInt(left) < GetInt(right));
                case "int > int": return MakeBool(tk, GetInt(left) > GetInt(right));
                case "int == int": return MakeBool(tk, GetInt(left) == GetInt(right));
                case "int != int": return MakeBool(tk, GetInt(left) != GetInt(right));

                case "int << int":
                    rightInt = GetInt(right);
                    if (rightInt < 0) throw new ParserException(tk, "Cannot bit shift by a negative number.");
                    return MakeInt(tk, GetInt(left) << rightInt);
                case "int >> int":
                    rightInt = GetInt(right);
                    if (rightInt < 0) throw new ParserException(tk, "Cannot bit shift by a negative number.");
                    return MakeInt(tk, GetInt(left) >> rightInt);

                case "int + float": return MakeFloat(tk, GetInt(left) + GetFloat(right));
                case "int - float": return MakeFloat(tk, GetInt(left) - GetFloat(right));
                case "int * float": return MakeFloat(tk, GetInt(left) * GetFloat(right));
                case "int / float": CheckZero(right); return MakeFloat(tk, GetInt(left) / GetFloat(right));
                case "int ** float":
                    rightFloat = GetFloat(right);
                    leftInt = GetInt(left);
                    if (rightFloat == 0) return MakeFloat(tk, 1);
                    return MakeFloat(tk, Math.Pow(leftInt, rightFloat));
                case "int % float": return MakeFloat(tk, PositiveModFloat(left, right));
                case "int <= float": return MakeBool(tk, GetInt(left) <= GetFloat(right));
                case "int >= float": return MakeBool(tk, GetInt(left) >= GetFloat(right));
                case "int < float": return MakeBool(tk, GetInt(left) < GetFloat(right));
                case "int > float": return MakeBool(tk, GetInt(left) > GetFloat(right));
                case "int == float": return MakeBool(tk, GetInt(left) == GetFloat(right));
                case "int != float": return MakeBool(tk, GetInt(left) != GetFloat(right));

                case "float + int": return MakeFloat(tk, GetFloat(left) + GetInt(right));
                case "float - int": return MakeFloat(tk, GetFloat(left) - GetInt(right));
                case "float * int": return MakeFloat(tk, GetFloat(left) * GetInt(right));
                case "float / int": CheckZero(right); return MakeFloat(tk, GetFloat(left) / GetInt(right));
                case "float ** int":
                    rightInt = GetInt(right);
                    leftFloat = GetFloat(left);
                    if (rightInt == 0) return MakeFloat(tk, 1);
                    return MakeFloat(tk, Math.Pow(leftFloat, rightInt));
                case "float % int": return MakeFloat(tk, PositiveModFloat(left, right));
                case "float <= int": return MakeBool(tk, GetFloat(left) <= GetInt(right));
                case "float >= int": return MakeBool(tk, GetFloat(left) >= GetInt(right));
                case "float < int": return MakeBool(tk, GetFloat(left) < GetInt(right));
                case "float > int": return MakeBool(tk, GetFloat(left) > GetInt(right));
                case "float == int": return MakeBool(tk, GetFloat(left) == GetInt(right));
                case "float != int": return MakeBool(tk, GetFloat(left) != GetInt(right));

                case "float + float": return MakeFloat(tk, GetFloat(left) + GetFloat(right));
                case "float - float": return MakeFloat(tk, GetFloat(left) - GetFloat(right));
                case "float * float": return MakeFloat(tk, GetFloat(left) * GetFloat(right));
                case "float / float": CheckZero(right); return MakeFloat(tk, GetFloat(left) / GetFloat(right));
                case "float ** float":
                    rightFloat = GetFloat(right);
                    leftFloat = GetFloat(left);
                    if (rightFloat == 0) return MakeFloat(tk, 1);
                    return MakeFloat(tk, Math.Pow(leftFloat, rightFloat));
                case "float % float": return MakeFloat(tk, PositiveModFloat(left, right));
                case "float <= float": return MakeBool(tk, GetFloat(left) <= GetFloat(right));
                case "float >= float": return MakeBool(tk, GetFloat(left) >= GetFloat(right));
                case "float < float": return MakeBool(tk, GetFloat(left) < GetFloat(right));
                case "float > float": return MakeBool(tk, GetFloat(left) > GetFloat(right));
                case "float == float": return MakeBool(tk, GetFloat(left) == GetFloat(right));
                case "float != float": return MakeBool(tk, GetFloat(left) != GetFloat(right));

                case "bool + string": return MakeString(tk, GetBool(left).ToString() + GetString(right));
                case "int + string": return MakeString(tk, GetInt(left).ToString() + GetString(right));
                case "float + string": return MakeString(tk, GetFloatAsString(left) + GetString(right));

                case "string + bool": return MakeString(tk, GetString(left) + GetBool(right).ToString());
                case "string + int": return MakeString(tk, GetString(left) + GetInt(right).ToString());
                case "string + float": return MakeString(tk, GetString(left) + GetFloatAsString(right));

                case "string + string": return MakeString(tk, GetString(left) + GetString(right));

                case "string * int":
                case "int * string":
                    string stringValue = (leftType == "string") ? GetString(left) : GetString(right);
                    int intValue = (leftType == "string") ? GetInt(right) : GetInt(left);

                    // don't consolidate this operation if it's going to use a bunch of memory.
                    if (intValue * stringValue.Length <= 100)
                    {
                        string output = "";
                        while (intValue > 0)
                        {
                            output += stringValue;
                            --intValue;
                        }
                        return MakeString(tk, output);
                    }

                    return this;

                case "string == string": return MakeBool(tk, GetString(left) == GetString(right));
                case "string != string": return MakeBool(tk, GetString(left) != GetString(right));

                default:
                    throw new ParserException(opToken, "This operator is invalid for types: " + leftType + ", " + rightType + ".");
            }
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
                isZero = ((FloatConstant)expr).Value == 0;
            }
            if (isZero)
            {
                throw new ParserException(expr.FirstToken, "Division by 0 error.");
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

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableIdAllocator varIds, VariableIdAllocPhase phase)
        {
            if ((phase & VariableIdAllocPhase.ALLOC) != 0)
            {
                this.Left.PerformLocalIdAllocation(parser, varIds, phase);
                this.Right.PerformLocalIdAllocation(parser, varIds, phase);
            }
        }

        internal override Expression ResolveNames(ParserContext parser)
        {
            this.Left = this.Left.ResolveNames(parser);
            this.Right = this.Right.ResolveNames(parser);
            return this;
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars)
        {
            this.Left.GetAllVariablesReferenced(vars);
            this.Right.GetAllVariablesReferenced(vars);
        }
    }
}
