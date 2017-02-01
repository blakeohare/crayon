using System;
using System.Collections.Generic;

namespace Pastel.Nodes
{
    class BreakStatement : Executable
    {
        public BreakStatement(Token breakToken) : base(breakToken)
        { }

        public override IList<Executable> ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            return this.Listify(this);
        }
    }
}
