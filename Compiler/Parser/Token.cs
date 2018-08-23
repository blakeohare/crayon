﻿namespace Parser
{
    public class Token
    {
        public string Value { get; private set; }
        public int Line { get; private set; }
        public int Col { get; private set; }
        public int FileID { get; private set; }
        public string FileName { get; private set; }
        public bool HasWhitespacePrefix { get; private set; }

        internal Token(string value, int fileID, string filename, int lineIndex, int colIndex, bool hasWhitespacePrefix)
        {
            this.Value = value;
            this.FileID = fileID;
            this.FileName = filename;
            this.Line = lineIndex;
            this.Col = colIndex;
            this.HasWhitespacePrefix = hasWhitespacePrefix;
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
