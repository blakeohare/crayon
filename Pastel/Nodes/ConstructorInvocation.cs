using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pastel.Nodes
{
    public class ConstructorInvocation : Expression
    {
        public PType Type { get; set; }
        public Expression[] Args { get; set; }
        public StructDefinition StructType { get; set; }

        public ConstructorInvocation(Token firstToken, PType type, IList<Expression> args) : base(firstToken)
        {
            this.Type = type;
            this.Args = args.ToArray();
            this.ResolvedType = type;
        }

        public override Expression ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            throw new NotImplementedException();
        }

        internal override Expression ResolveType(VariableScope varScope, PastelCompiler compiler)
        {
            throw new NotImplementedException();
        }

        internal override Expression ResolveWithTypeContext(PastelCompiler compiler)
        {
            for (int i = 0; i < this.Args.Length; ++i)
            {
                this.Args[i] = this.Args[i].ResolveWithTypeContext(compiler);
            }

            string type = this.Type.RootValue;
            switch (type)
            {
                case "Array":
                case "List":
                case "Dictionary":
                    break;
                default:
                    StructDefinition sd = compiler.GetStructDefinition(this.Type.RootValue);
                    if (sd != null)
                    {
                        this.StructType = sd;
                    }
                    break;
            }

            return this;
        }
    }
}
