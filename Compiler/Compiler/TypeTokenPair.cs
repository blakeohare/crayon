namespace Parser
{
    internal class TypeTokenPair
    {
        public AType Type { get; set; }
        public Token Token { get; set; }

        public TypeTokenPair(AType type, Token token)
        {
            this.Type = type;
            this.Token = token;
        }
    }
}
