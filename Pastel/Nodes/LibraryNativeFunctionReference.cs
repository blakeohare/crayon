using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pastel.Nodes
{
    class LibraryNativeFunctionReference : Expression
    {
        public string Name { get; set; }

        public LibraryNativeFunctionReference(Token token, string name) : base(token)
        {
            this.Name = name;
        }

        public override Expression ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            throw new NotImplementedException();
        }

        internal override Expression ResolveType(VariableScope varScope, PastelCompiler compiler)
        {
            return this;
        }
    }
}
