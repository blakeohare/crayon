namespace Crayon
{
	internal class Token
	{
		public string Value { get; private set; }
		public int Line { get; private set; }
		public int Col { get; private set; }
		public int FileID { get; private set; }
		public string FileName { get; private set; }
		public bool HasWhitespacePrefix { get; private set; }

		public Token(string value, int fileID, string filename, int lineIndex, int colIndex, bool hasWhitespacePrefix)
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
	}
}
