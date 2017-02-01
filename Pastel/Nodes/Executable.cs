using System;
using System.Collections.Generic;

namespace Pastel.Nodes
{
    public abstract class Executable
    {
        public Token FirstToken { get; set; }

        public Executable(Token firstToken)
        {
            this.FirstToken = firstToken;
        }

        public abstract IList<Executable> ResolveNamesAndCullUnusedCode(PastelCompiler compiler);

        protected IList<Executable> Listify(Executable ex)
        {
            return new Executable[] { ex };
        }

        internal static IList<Executable> ResolveNamesAndCullUnusedCodeForBlock(IList<Executable> code, PastelCompiler compiler)
        {
            List<Executable> output = new List<Executable>();
            for (int i = 0; i < code.Count; ++i)
            {
                output.AddRange(code[i].ResolveNamesAndCullUnusedCode(compiler));
            }
            return output;
        }
    }
}
