namespace Crayon.Pastel.Nodes
{
    class Executable
    {
        public Token FirstToken { get; private set; }

        public Executable(Token firstToken)
        {
            this.FirstToken = firstToken;
        }
    }
}
