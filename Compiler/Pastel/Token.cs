namespace Pastel
{
    public class Token
    {
        public string Value { get; private set; }
        public int Line { get; private set; }
        public int Col { get; private set; }
        public string FileName { get; private set; }
        public bool HasWhitespacePrefix { get; private set; }

        public Token(string value, string filename, int lineIndex, int colIndex, bool hasWhitespacePrefix)
        {
            this.Value = value;
            this.FileName = filename;
            this.Line = lineIndex;
            this.Col = colIndex;
            this.HasWhitespacePrefix = hasWhitespacePrefix;
        }

        public static Token CreateDummyToken(string value)
        {
            return new Token(value, "", 0, 0, false);
        }

        public override string ToString()
        {
            return "Token: '" + this.Value + "'";
        }
    }
}
