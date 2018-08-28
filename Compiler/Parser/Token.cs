namespace Parser
{
    public class Token
    {
        public string Value { get; private set; }
        public int Line { get; private set; }
        public int Col { get; private set; }
        public int FileID { get { return this.File.ID; } }
        public string FileName { get { return this.File.Name; } }
        public TokenType Type { get; private set; }
        public FileScope File { get; private set; }

        internal Token(string value, TokenType type, FileScope file, int lineIndex, int colIndex)
        {
            this.Value = value;
            this.Type = type;
            this.File = file;
            this.Line = lineIndex;
            this.Col = colIndex;
        }

        public override string ToString()
        {
            return "Token: '" + this.Value + "'";
        }

        internal bool IsInteger
        {
            get
            {
                foreach (char c in this.Value)
                {
                    if (c < '0' || c > '9')
                    {
                        return false;
                    }
                }
                return true;
            }
        }
    }
}
