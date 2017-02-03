using System;
using System.Collections.Generic;
using System.Linq;

namespace Pastel.Nodes
{
    class OpChain : Expression
    {
        public Expression[] Expressions { get; set; }
        public Token[] Ops { get; set; }

        public OpChain(
            IList<Expression> expressions,
            IList<Token> ops) : base(expressions[0].FirstToken)
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
                        current = new InlineConstant(PType.INT, current.FirstToken, (int)current.Value + (int)next.Value);
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
                        this.ResolvedType = PType.BOOL;
                        break;

                    default:
                        throw new ParserException(this.Ops[i], "The operator '" + this.Ops[i].Value + "' is not defined for types: " + this.ResolvedType + " and " + nextType + ".");
                }
            }
            return this;
        }
    }
}
