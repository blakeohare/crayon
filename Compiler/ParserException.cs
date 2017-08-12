using System;

namespace Crayon
{
    class ParserException : Exception
    {
        public ParserException(Token token, string message)
            : base(InterpretToken(token) + message)
        { }

        private ParserException(string message) : base(message)
        { }

        public static ParserException ThrowEofException(string filename)
        {
            throw new ParserException("Unexpected EOF in " + filename);
        }

        private static string InterpretToken(Token token)
        {
            if (token == null) return "";
            return token.FileName + ", Line: " + (token.Line + 1) + ", Col: " + (token.Col + 1) + ", ";
        }
    }
}
