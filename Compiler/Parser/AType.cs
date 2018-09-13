using System.Collections.Generic;
using System.Linq;

namespace Parser
{
    public class AType
    {
        public Token FirstToken { get; set; }
        public Token[] RootTypeTokens { get; set; }
        public string RootType { get; set; }
        public AType[] Generics { get; set; }

        public AType(IList<Token> rootType, IList<AType> generics)
        {
            this.RootTypeTokens = rootType.ToArray();
            this.FirstToken = this.RootTypeTokens[0];
            this.RootType = string.Join(".", this.RootTypeTokens.Select(t => t.Value));
            this.Generics = generics.ToArray();
        }
    }
}
