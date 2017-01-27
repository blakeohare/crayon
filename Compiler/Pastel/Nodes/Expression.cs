using System.Collections.Generic;

namespace Crayon.Pastel.Nodes
{
    abstract class Expression
    {
        public Token FirstToken { get; private set; }

        public PType ResolvedType { get; set; }

        public Expression(Token firstToken)
        {
            this.FirstToken = firstToken;
        }

        public abstract Expression NameResolution(
            Dictionary<string, FunctionDefinition> functionLookup,
            Dictionary<string, StructDefinition> structLookup);

        public abstract void ResolveTypes();
    }
}
