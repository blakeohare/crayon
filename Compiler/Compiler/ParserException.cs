using System;
using System.Collections.Generic;
using System.Linq;
using Wax;

namespace Builder
{
    internal class MultiParserException : Exception
    {
        public ParserException[] ParseExceptions { get; private set; }

        public MultiParserException(IList<ParserException> exceptions)
            : base(string.Join("\n\n", exceptions.Select(ex => ex.Message)))
        {
            this.ParseExceptions = exceptions.ToArray();
        }

        internal Error[] ToCompilerErrors()
        {
            return this.ParseExceptions.Select(err => err.ToCompilerError()).ToArray();
        }
    }

    internal class ParserException : Exception
    {
        public string OriginalMessage { get; private set; }

        private FileScope file = null;
        public string FileName
        {
            get
            {
                if (this.TokenInfo != null) return this.TokenInfo.File.Name;
                if (this.file != null) return this.file.Name;
                return null;
            }
        }
        public Token TokenInfo { get; set; }

        internal ParserException(Builder.Localization.ErrorMessages errorType, Token token, Builder.Localization.Locale locale)
            : this(token, locale.Strings.Get(errorType.ToString()))
        {
            this.TokenInfo = token;
            this.OriginalMessage = locale.Strings.Get(errorType.ToString());
        }

        internal ParserException(ParseTree.Node node, string message)
            : this(node.FirstToken, message)
        {
            this.TokenInfo = node.FirstToken;
            this.OriginalMessage = message;
        }

        internal ParserException(Token token, string message)
            : base(InterpretToken(token) + message)
        {
            this.TokenInfo = token;
            this.OriginalMessage = message;
        }

        internal ParserException(FileScope file, string message)
            : base(file.Name + ": " + message)
        {
            this.file = file;
            this.OriginalMessage = message;
        }

        internal ParserException(string message) : base(message)
        {
            this.OriginalMessage = message;
        }

        internal static ParserException ThrowEofException(string filename)
        {
            return ThrowEofExceptionWithSuggestion(filename, "Did you forget a closing parenthesis or curly brace?");
        }

        internal static ParserException ThrowEofExceptionWithSuggestion(string filename, string suggestion)
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
        internal static ParserException ThrowException(Builder.Localization.Locale locale, Builder.Localization.ErrorMessages errorType, Token token, params string[] args)
        {
            throw new ParserException(token, locale.Strings.Get(errorType.ToString(), args));
        }

        internal Error ToCompilerError()
        {
            Error error = new Error();
            if (this.TokenInfo != null)
            {
                error.Column = this.TokenInfo.Col + 1;
                error.Line = this.TokenInfo.Line + 1;
            }
            error.FileName = this.FileName;
            error.Message = this.OriginalMessage;
            return error;
        }
    }
}
