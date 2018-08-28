﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Parser
{
    public class MultiParserException : Exception
    {
        public MultiParserException(IList<ParserException> exceptions)
            : base(string.Join("\n\n", exceptions.Select(ex => ex.Message)))
        { }
    }

    public class ParserException : Exception
    {
        public ParserException(Localization.ErrorMessages errorType, Token token, Localization.Locale locale)
            : this(token, locale.Strings.Get(errorType.ToString()))
        { }

        public ParserException(ParseTree.Node node, string message)
            : this(node.FirstToken, message)
        { }

        public ParserException(Token token, string message)
            : base(InterpretToken(token) + message)
        { }

        public ParserException(FileScope file, string message)
            : base(file.Name + ": " + message)
        { }

        internal ParserException(string message) : base(message)
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

        // Throws an exception, but also returns an exception so that you can use 'throw' syntax
        // at the calling site to prevent the compiler from complaining about control flow branches
        // that aren't actually accessible.
        public static ParserException ThrowException(Localization.Locale locale, Localization.ErrorMessages errorType, Token token, params string[] args)
        {
            throw new ParserException(token, locale.Strings.Get(errorType.ToString(), args));
        }
    }
}
