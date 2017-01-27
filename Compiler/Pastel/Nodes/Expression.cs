namespace Crayon.Pastel.Nodes
{
    class Expression
    {
        public Token FirstToken { get; private set; }

        public Expression(Token firstToken)
        {
            this.FirstToken = firstToken;
        }
    }
}
