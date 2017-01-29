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


        public abstract IList<Executable> NameResolution(
            Dictionary<string, FunctionDefinition> functionLookup,
            Dictionary<string, StructDefinition> structLookup);

        public abstract void ResolveTypes();
    }
}
