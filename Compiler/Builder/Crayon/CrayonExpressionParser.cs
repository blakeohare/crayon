using Builder.ParseTree;
using System.Collections.Generic;

namespace Builder.Crayon
{
    internal class CrayonExpressionParser : AbstractExpressionParser
    {
        public CrayonExpressionParser(ParserContext parser)
            : base(parser)
        { }

        protected override AType ParseTypeForInstantiation(TokenStream tokens)
        {
            tokens.EnsureNotEof();
            Token throwToken = tokens.Peek();
            AType type = this.parser.TypeParser.TryParse(tokens);
            if (type == null || type.Generics.Length > 0)
            {
                throw new ParserException(throwToken, "This is not a valid class name.");
            }
            return type;
        }

        protected override Expression ParseInstantiate(TokenStream tokens, Node owner)
        {
            Token newToken = tokens.PopExpected(this.parser.Keywords.NEW);
            AType className = this.ParseTypeForInstantiation(tokens);
            IList<Expression> args = this.ParseArgumentList(tokens, owner);
            return new Instantiate(newToken, className.FirstToken, className.RootType, className.Generics, args, owner);
        }

        protected override AType MaybeParseCastPrefix(TokenStream tokens)
        {
            // Casting not supported in Crayon.
            return null;
        }
    }
}
