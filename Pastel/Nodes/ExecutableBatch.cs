using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pastel.Nodes
{
    public class ExecutableBatch : Executable
    {
        public Executable[] Executables { get; set; }
        public ExecutableBatch(Token firstToken, IList<Executable> executables) : base(firstToken)
        {
            this.Executables = executables.ToArray();
        }

        public override IList<Executable> ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            return this.Executables;
        }

        internal override void ResolveTypes(VariableScope varScope, PastelCompiler compiler)
        {
            for (int i = 0; i < this.Executables.Length; ++i)
            {
                this.Executables[i].ResolveTypes(varScope, compiler);
            }
        }

        internal override Executable ResolveWithTypeContext(PastelCompiler compiler)
        {
            Executable.ResolveWithTypeContext(compiler, this.Executables);
            return this;
        }
    }
}
