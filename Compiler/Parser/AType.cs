using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    public class AType
    {
        public Token RootTypeToken { get; set; }
        public string RootType { get; set; }
        public AType[] Generics { get; set; }

        public AType(Token rootType, IList<AType> generics)
        {
            this.RootTypeToken = rootType;
            this.RootType = rootType.Value;
            this.Generics = generics.ToArray();
        }
    }
}
