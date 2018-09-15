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

        private static readonly AType[] EMPTY_TYPE_ARGS = new AType[0];

        public AType(IList<Token> rootType) : this(rootType, EMPTY_TYPE_ARGS) { }

        public AType(IList<Token> rootType, IList<AType> generics)
        {
            this.RootTypeTokens = rootType.ToArray();
            this.FirstToken = this.RootTypeTokens[0];
            this.RootType = string.Join(".", this.RootTypeTokens.Select(t => t.Value));
            this.Generics = generics.ToArray();
        }

        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("AType: ");
            this.ToStringImpl(sb);
            return sb.ToString();
        }

        private void ToStringImpl(System.Text.StringBuilder sb)
        {
            if (this.RootTypeTokens[0].Value == "[")
            {
                this.Generics[0].ToStringImpl(sb);
                sb.Append("[]");
            }
            else
            {
                sb.Append(this.RootType);
                if (this.Generics.Length > 0)
                {
                    sb.Append('<');
                    for (int i = 0; i < this.Generics.Length; ++i)
                    {
                        if (i > 0) sb.Append(", ");
                        this.Generics[i].ToStringImpl(sb);
                    }
                    sb.Append('>');
                }
            }
        }
    }
}
