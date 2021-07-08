namespace Common
{
    public class Error
    {
        public int Line { get; set; }
        public int Column { get; set; }
        public string FileName { get; set; }
        public string Message { get; set; }
        public bool HasLineInfo { get { return this.Line != 0; } }

        public static string ToJson(Error[] errors)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("{ \"errors\": [");
            for (int i = 0; i < errors.Length; ++i)
            {
                Error error = errors[i];
                if (i > 0) sb.Append(',');

                sb.Append("\n  {");
                if (error.FileName != null)
                {
                    sb.Append("\n    \"file\": \"");
                    sb.Append(error.FileName.Replace("\\", "\\\\"));
                    sb.Append("\",");
                }

                if (error.HasLineInfo)
                {
                    sb.Append("\n    \"col\": ");
                    sb.Append(error.Column + 1);
                    sb.Append(",");
                    sb.Append("\n    \"line\": ");
                    sb.Append(error.Line + 1);
                    sb.Append(",");
                }
                sb.Append("\n    \"message\": \"");
                sb.Append(error.Message.Replace("\\", "\\\\").Replace("\"", "\\\""));
                sb.Append("\"\n  }");
            }
            sb.Append(" ] }");
            return sb.ToString();
        }
    }
}
