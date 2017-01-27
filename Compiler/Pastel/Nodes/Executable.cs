using System.Collections.Generic;

namespace Crayon.Pastel.Nodes
{
    abstract class Executable
    {
        public Token FirstToken { get; private set; }

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
