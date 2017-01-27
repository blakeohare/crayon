using System.Collections.Generic;
using System.Linq;

namespace Crayon.Pastel.Nodes
{
    class ConstructorReference : Expression
    {
        public Token[] DotChain { get; set; }

        public ConstructorReference(Token newToken, IList<Token> dotChain) : base(newToken)
        {
            this.DotChain = dotChain.ToArray();
        }
    }
}
