using System.Collections.Generic;

namespace Pastel.Nodes
{
    internal class ExecutableBatch : Executable
    {
        public Executable[] Executables { get; set; }
        public ExecutableBatch(Token firstToken, IList<Executable> executables) : base(firstToken)
        {
            List<Executable> items = new List<Executable>();
            this.AddAllItems(items, executables);
            this.Executables = items.ToArray();
        }

        private void AddAllItems(List<Executable> items, IList<Executable> executables)
        {
            Executable item;
            int length = executables.Count;
            for (int i = 0; i < length; ++i)
            {
                item = executables[i];
                if (item is ExecutableBatch)
                {
                    this.AddAllItems(items, ((ExecutableBatch)item).Executables);
                }
                else
                {
                    items.Add(item);
                }
            }
        }

        public override Executable ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            List<Executable> executables = new List<Executable>();
            for (int i = 0; i < this.Executables.Length; ++i)
            {
                Executable exec = this.Executables[i].ResolveNamesAndCullUnusedCode(compiler);
                if (exec is ExecutableBatch)
                {
                    executables.AddRange(((ExecutableBatch)exec).Executables);
                }
                else
                {
                    executables.Add(exec);
                }
            }

            if (executables.Count == 1)
            {
                return executables[0];
            }

            this.Executables = executables.ToArray();
            return this;
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
