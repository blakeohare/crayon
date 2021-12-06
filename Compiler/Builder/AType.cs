using System.Collections.Generic;
using System.Linq;

namespace Builder
{
    internal class AType
    {
        public Token FirstToken { get; set; }
        public Token[] RootTypeTokens { get; set; }
        public string RootType { get; set; }
        public AType[] Generics { get; set; }
        public bool IsAnyType { get; set; }

        private static readonly Token[] EMPTY_TOKEN_LIST = new Token[0];
        private static readonly AType[] EMPTY_TYPE_ARGS = new AType[0];

        private static AType Empty(Token nullableToken)
        {
            AType t = new AType(EMPTY_TOKEN_LIST);
            t.FirstToken = nullableToken;
            return t;
        }

        public static AType Any()
        {
            return Any(null);
        }

        public static AType Any(Token token)
        {
            AType type = Empty(token);
            type.IsAnyType = true;
            return type;
        }

        public static AType Integer(Token token)
        {
            AType t = Empty(token);
            t.RootType = "int";
            return t;
        }

        public static AType ProvideRoot(Token token, string rootType)
        {
            AType type = Empty(token);
            type.RootType = rootType;
            return type;
        }

        public AType(IList<Token> rootType) : this(rootType, EMPTY_TYPE_ARGS) { }

        public AType(IList<Token> rootType, IList<AType> generics)
        {
            this.IsAnyType = false;
            this.RootTypeTokens = rootType.ToArray();
            this.FirstToken = this.RootTypeTokens.Length == 0 ? null : this.RootTypeTokens[0];
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
