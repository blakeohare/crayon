using System;
using System.Collections.Generic;

namespace Pastel.Nodes
{
    class BracketIndex : Expression
    {
        public Expression Root { get; set; }
        public Token BracketToken { get; set; }
        public Expression Index { get; set; }

        public BracketIndex(Expression root, Token bracketToken, Expression index) : base(root.FirstToken)
        {
            this.Root = root;
            this.BracketToken = bracketToken;
            this.Index = index;
        }

        public override Expression ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            this.Root = this.Root.ResolveNamesAndCullUnusedCode(compiler);
            this.Index = this.Index.ResolveNamesAndCullUnusedCode(compiler);
            return this;
        }

        internal override Expression ResolveType(VariableScope varScope, PastelCompiler compiler)
        {
            this.Root.ResolveType(varScope, compiler);
            this.Index.ResolveType(varScope, compiler);

            PType rootType = this.Root.ResolvedType;
            PType indexType = this.Index.ResolvedType;

            bool badIndex = false;
            if (rootType.RootValue == "List" || rootType.RootValue == "Array")
            {
                badIndex = !indexType.IsIdentical(PType.INT);
                this.ResolvedType = rootType.Generics[0];
            }
            else if (rootType.RootValue == "Dictionary")
            {
                badIndex = !indexType.IsIdentical(rootType.Generics[0]);
                this.ResolvedType = rootType.Generics[1];
            }
            else if (rootType.RootValue == "string")
            {
                badIndex = indexType.IsIdentical(PType.INT);
                this.ResolvedType = PType.CHAR;
            }
            else
            {
                badIndex = true;
            }

            if (badIndex)
            {
                throw new ParserException(this.BracketToken, "Cannot index into a " + rootType + " with a " + indexType + ".");
            }

            return this;
        }
    }
}
