using System;

namespace Crayon
{
	class ParserException : Exception
	{
		public ParserException(Token token, string message)
			: base(InterpretToken(token) + message)
		{ }

		private static string InterpretToken(Token token)
		{
			return "Line: " + (token.Line + 1) + ", Col: " + (token.Col + 1) + ", ";
		}
	}
}
