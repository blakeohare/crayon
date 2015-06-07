using System.Text;

namespace Crayon.ParseTree
{
	internal class StringConstant : Expression
	{
		public string Value { get; private set; }
		public StringConstant(Token token, string value)
			: base(token)
		{
			this.Value = value;
		}

		public override bool IsLiteral { get { return true; } }

		public static string ParseOutRawValue(Token stringToken)
		{
			string rawValue = stringToken.Value;
			rawValue = rawValue.Substring(1, rawValue.Length - 2);
			StringBuilder sb = new StringBuilder();
			char c;
			for (int i = 0; i < rawValue.Length; ++i)
			{
				c = rawValue[i];
				if (c == '\\')
				{
					c = rawValue[++i];
					switch (c)
					{
						case 'n': sb.Append('\n'); break;
						case 'r': sb.Append('\r'); break;
						case '0': sb.Append('\0'); break;
						case 't': sb.Append('\t'); break;
						case 'b': sb.Append('\b'); break;
						case '\'': sb.Append("'"); break;
						case '"': sb.Append("\""); break;
						case '\\': sb.Append("\\"); break;
						default: throw new ParserException(stringToken, "Invalid escape sequence: \\" + c);
					}
				}
				else
				{
					sb.Append(c);
				}
			}

			return sb.ToString();
		}

		public override Expression Resolve(Parser parser)
		{
			return this;
		}
	}
}
