using System.Collections.Generic;

namespace Builder
{
    internal abstract class AbstractTypeParser
    {
        public virtual AType TryParse(TokenStream tokens)
        {
            TokenStream.StreamState tss = tokens.RecordState();
            AType type = this.TryParseImpl(tokens);
            if (type != null) return type;
            tokens.RestoreState(tss);
            return null;
        }

        private AType TryParseImpl(TokenStream tokens)
        {
            Token firstPart = tokens.PopIfWord();
            if (firstPart == null) return null;

            List<Token> parts = new List<Token>() { firstPart };
            while (tokens.PopIfPresent("."))
            {
                Token part = tokens.PopIfWord();
                if (part == null) return null;
                parts.Add(part);
            }
            return new AType(parts);
        }

        public virtual AType Parse(TokenStream tokens)
        {
            tokens.EnsureNotEof();
            Token throwToken = tokens.Peek();
            AType type = this.TryParse(tokens);
            if (type == null) throw new ParserException(throwToken, "Type name expected");
            return type;
        }
    }
}
