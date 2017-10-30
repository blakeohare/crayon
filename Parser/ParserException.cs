using System;

namespace Parser
{
    public class ParserException : Exception
    {
        public ParserException(Token token, string message)
            : base(InterpretToken(token) + message)
        { }

        private ParserException(string message) : base(message)
        { }

        public static ParserException ThrowEofException(string filename)
        {
            return ThrowEofExceptionWithSuggestion(filename, "Did you forget a closing parenthesis or curly brace?");
        }

        public static ParserException ThrowEofExceptionWithSuggestion(string filename, string suggestion)
        {
            throw new ParserException("Unexpected EOF in " + filename + ". " + suggestion);
        }

        private static string InterpretToken(Token token)
        {
            if (token == null) return "";
            return token.FileName + ", Line: " + (token.Line + 1) + ", Col: " + (token.Col + 1) + ", ";
        }
    }
}
