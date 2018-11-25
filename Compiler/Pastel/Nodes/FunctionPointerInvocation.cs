using System;
using System.Collections.Generic;
using System.Linq;

namespace Pastel.Nodes
{
    internal class FunctionPointerInvocation : Expression
    {
        public Expression Root { get; private set; }
        public Expression[] Args { get; private set; }

        // Note that this class is instantiated in the ResolveType phase.
        public FunctionPointerInvocation(PastelCompiler compiler, Token firstToken, Expression root, IList<Expression> Args)
            : base(firstToken, root.Owner)
        {
            this.Root = root;
            this.Args = Args.ToArray();

            this.ResolvedType = this.Root.ResolvedType.Generics[0];

            if (this.Root.ResolvedType.Generics.Length - 1 != this.Args.Length)
            {
                throw new ParserException(this.Root.FirstToken, "This function has the incorrect number of arguments.");
            }
            for (int i = 0; i < this.Args.Length; ++i)
            {
                PType expectedArgType = this.Root.ResolvedType.Generics[i + 1];
                PType actualArgType = this.Args[i].ResolvedType;
                if (!actualArgType.IsIdentical(compiler, expectedArgType))
                {
                    throw new ParserException(this.Args[i].FirstToken, "Incorrect argument type. Expected " + expectedArgType + " but found " + actualArgType + ".");
                }
            }
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
            return this;
        }
    }
}
