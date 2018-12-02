using System;
using System.Collections.Generic;
using System.Linq;

namespace Pastel.Nodes
{
    internal class OpChain : Expression
    {
        public Expression[] Expressions { get; set; }
        public Token[] Ops { get; set; }

        public OpChain(
            IList<Expression> expressions,
            IList<Token> ops) : base(expressions[0].FirstToken, expressions[0].Owner)
        {
            this.Expressions = expressions.ToArray();
            this.Ops = ops.ToArray();
        }

        public override Expression ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            Expression.ResolveNamesAndCullUnusedCodeInPlace(this.Expressions, compiler);
            // Don't do short-circuiting yet for && and ||
            return this;
        }

        internal override InlineConstant DoConstantResolution(HashSet<string> cycleDetection, PastelCompiler compiler)
        {
            for (int i = 0; i < this.Expressions.Length; ++i)
            {
                this.Expressions[i] = this.Expressions[i].DoConstantResolution(cycleDetection, compiler);
            }

            InlineConstant current = (InlineConstant)this.Expressions[0];
            for (int i = 1; i < this.Expressions.Length; ++i)
            {
                InlineConstant next = (InlineConstant)this.Expressions[i];
                string lookup = current.Type.RootValue + this.Ops[i - 1].Value + next.Type.RootValue;
                switch (lookup)
                {
                    case "int+int":
                        current = new InlineConstant(PType.INT, current.FirstToken, (int)current.Value + (int)next.Value, next.Owner);
                        break;
                    case "int-int":
                        current = new InlineConstant(PType.INT, current.FirstToken, (int)current.Value - (int)next.Value, next.Owner);
                        break;
                    case "int*int":
                        current = new InlineConstant(PType.INT, current.FirstToken, (int)current.Value * (int)next.Value, next.Owner);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            return current;
        }

        internal override Expression ResolveType(VariableScope varScope, PastelCompiler compiler)
        {
            for (int i = 0; i < this.Expressions.Length; ++i)
            {
                this.Expressions[i] = this.Expressions[i].ResolveType(varScope, compiler);
            }

            this.ResolvedType = this.Expressions[0].ResolvedType;

            for (int i = 0; i < this.Ops.Length; ++i)
            {
                PType nextType = this.Expressions[i + 1].ResolvedType;
                string op = this.Ops[i].Value;
                if (op == "==" || op == "!=")
                {
                    if ((nextType.RootValue == this.ResolvedType.RootValue) ||
                        (nextType.RootValue == "null" && this.ResolvedType.IsNullable) ||
                        (nextType.IsNullable && this.ResolvedType.RootValue == "null") ||
                        (nextType.RootValue == "null" && this.ResolvedType.RootValue == "null"))
                    {
                        this.ResolvedType = PType.BOOL;
                        continue;
                    }
                }
                string lookup = this.ResolvedType.RootValue + this.Ops[i].Value + nextType.RootValue;
                switch (lookup)
                {
                    case "int+int":
                    case "int-int":
                    case "int*int":
                    case "int%int":
                    case "int&int":
                    case "int|int":
                    case "int^int":
                    case "int<<int":
                    case "int>>int":
                        this.ResolvedType = PType.INT;
                        break;

                    case "int+double":
                    case "double+int":
                    case "double+double":
                    case "int-double":
                    case "double-int":
                    case "double-double":
                    case "int*double":
                    case "double*int":
                    case "double*double":
                    case "double%int":
                    case "int%double":
                    case "double%double":
                        this.ResolvedType = PType.DOUBLE;
                        break;

                    case "int>int":
                    case "int<int":
                    case "int>=int":
                    case "int<=int":
                    case "double<int":
                    case "double>int":
                    case "double<=int":
                    case "double>=int":
                    case "int<double":
                    case "int>double":
                    case "int<=double":
                    case "int>=double":
                    case "double<double":
                    case "double>double":
                    case "double<=double":
                    case "double>=double":
                    case "int==int":
                    case "double==double":
                    case "int==double":
                    case "double==int":
                    case "int!=int":
                    case "double!=double":
                    case "int!=double":
                    case "double!=int":
                    case "bool&&bool":
                    case "bool||bool":
                    case "char>char":
                    case "char<char":
                    case "char>=char":
                    case "char<=char":
                        this.ResolvedType = PType.BOOL;
                        break;

                    case "int/int":
                    case "int/double":
                    case "double/int":
                    case "double/double":
                        throw new ParserException(this.Ops[i], "Due to varying platform behavior of / use Core.IntegerDivision(numerator, denominator) or Core.FloatDivision(numerator, denominator)");

                    default:
                        throw new ParserException(this.Ops[i], "The operator '" + this.Ops[i].Value + "' is not defined for types: " + this.ResolvedType + " and " + nextType + ".");
                }
            }
            return this;
        }

        internal override Expression ResolveWithTypeContext(PastelCompiler compiler)
        {
            for (int i = 0; i < this.Expressions.Length; ++i)
            {
                this.Expressions[i] = this.Expressions[i].ResolveWithTypeContext(compiler);
            }

            InlineConstant left = this.Expressions[0] as InlineConstant;
            InlineConstant right = this.Expressions[1] as InlineConstant;
            while (left != null && right != null)
            {
                object leftValue = left.Value;
                object rightValue = right.Value;
                string lookup = left.ResolvedType.RootValue + this.Ops[0].Value + right.ResolvedType.RootValue;
                switch (lookup)
                {
                    case "int+int": this.Expressions[0] = CreateInteger(left.FirstToken, (int)leftValue + (int)rightValue); break;
                    case "int-int": this.Expressions[0] = CreateInteger(left.FirstToken, (int)leftValue - (int)rightValue); break;
                    case "int*int": this.Expressions[0] = CreateInteger(left.FirstToken, (int)leftValue * (int)rightValue); break;
                    case "int/int": this.Expressions[0] = CreateInteger(left.FirstToken, (int)leftValue / (int)rightValue); break;
                    case "int+double": this.Expressions[0] = CreateFloat(left.FirstToken, (int)leftValue + (double)rightValue); break;
                    case "int-double": this.Expressions[0] = CreateFloat(left.FirstToken, (int)leftValue - (double)rightValue); break;
                    case "int*double": this.Expressions[0] = CreateFloat(left.FirstToken, (int)leftValue * (double)rightValue); break;
                    case "int/double": this.Expressions[0] = CreateFloat(left.FirstToken, (int)leftValue / (double)rightValue); break;
                    case "double+int": this.Expressions[0] = CreateFloat(left.FirstToken, (double)leftValue + (int)rightValue); break;
                    case "double-int": this.Expressions[0] = CreateFloat(left.FirstToken, (double)leftValue - (int)rightValue); break;
                    case "double*int": this.Expressions[0] = CreateFloat(left.FirstToken, (double)leftValue * (int)rightValue); break;
                    case "double/int": this.Expressions[0] = CreateFloat(left.FirstToken, (double)leftValue / (int)rightValue); break;
                    case "double+double": this.Expressions[0] = CreateFloat(left.FirstToken, (double)leftValue + (double)rightValue); break;
                    case "double-double": this.Expressions[0] = CreateFloat(left.FirstToken, (double)leftValue - (double)rightValue); break;
                    case "double*double": this.Expressions[0] = CreateFloat(left.FirstToken, (double)leftValue * (double)rightValue); break;
                    case "double/double": this.Expressions[0] = CreateFloat(left.FirstToken, (double)leftValue / (double)rightValue); break;
                    case "bool&&bool": this.Expressions[0] = CreateBoolean(left.FirstToken, (bool)leftValue && (bool)rightValue); break;
                    case "bool||bool": this.Expressions[0] = CreateBoolean(left.FirstToken, (bool)leftValue || (bool)rightValue); break;
                    default:
                        if (this.Ops[0].Value == "%")
                        {
                            throw new NotImplementedException("Remember when you implement this to prevent negatives.");
                        }
                        throw new ParserException(this.Ops[0], "The operator is not defined for these two constants.");

                }
                List<Expression> expressions = new List<Expression>(this.Expressions);
                expressions.RemoveAt(1); // I know, I know...
                this.Expressions = expressions.ToArray();

                if (this.Expressions.Length == 1)
                {
                    return this.Expressions[0];
                }
                List<Token> ops = new List<Token>(this.Ops);
                ops.RemoveAt(0);
                this.Ops = ops.ToArray();
                left = this.Expressions[0] as InlineConstant;
                right = this.Expressions[1] as InlineConstant;
            }

            // TODO(pastel-split): This is just a quick and dirty short-circuit logic for && and ||
            // Do full logic later. Currently this is causing problems in specific snippets in Crayon libraries.
            string opValue = this.Ops[0].Value;
            if (left != null)
            {
                if (opValue == "&&" && left.Value is bool)
                {
                    return (bool)left.Value ? (Expression)this : left;
                }
                if (opValue == "||" && left.Value is bool)
                {
                    return (bool)left.Value ? left : (Expression)this;
                }
            }

            return this;
        }

        private InlineConstant CreateBoolean(Token originalFirstToken, bool value)
        {
            return new InlineConstant(PType.BOOL, originalFirstToken, value, this.Owner) { ResolvedType = PType.BOOL };
        }

        private InlineConstant CreateInteger(Token originalFirstToken, int value)
        {
            return new InlineConstant(PType.INT, originalFirstToken, value, this.Owner) { ResolvedType = PType.INT };
        }

        private InlineConstant CreateFloat(Token originalFirstToken, double value)
        {
            return new InlineConstant(PType.DOUBLE, originalFirstToken, value, this.Owner) { ResolvedType = PType.DOUBLE };
        }

        private InlineConstant CreateString(Token originalFirstToken, string value)
        {
            return new InlineConstant(PType.STRING, originalFirstToken, value, this.Owner) { ResolvedType = PType.STRING };
        }
    }
}
