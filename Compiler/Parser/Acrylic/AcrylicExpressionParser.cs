using Parser.ParseTree;
using System.Collections.Generic;

namespace Parser.Acrylic
{
    internal class AcrylicExpressionParser : AbstractExpressionParser
    {
        public AcrylicExpressionParser(ParserContext parser)
            : base(parser)
        { }

        protected override AType ParseTypeForInstantiation(TokenStream tokens)
        {
            return this.parser.TypeParser.Parse(tokens);
        }

        protected override Expression ParseInstantiate(TokenStream tokens, Node owner)
        {
            Token newToken = tokens.PopExpected(this.parser.Keywords.NEW);

            AType className = this.ParseTypeForInstantiation(tokens);

            if (className.RootType == "[" && tokens.IsNext("{"))
            {
                List<Expression> items = this.ParseArrayDeclarationItems(tokens, owner);

                // Use normal list for now just to get things working.
                // TODO: Introducing native array types.
                return new ListDefinition(newToken, items, className.Generics[0], owner);
            }
            else
            {
                IList<Expression> args = this.ParseArgumentList(tokens, owner);

                return new Instantiate(newToken, className.FirstToken, className.RootType, className.Generics, args, owner);
            }
        }

        private List<Expression> ParseArrayDeclarationItems(TokenStream tokens, Node owner)
        {
            List<Expression> output = new List<Expression>();
            tokens.PopExpected("{");
            bool nextItemAllowed = true;
            while (!tokens.PopIfPresent("}"))
            {
                if (!nextItemAllowed) tokens.PopExpected("}"); // throws reasonably-worded error

                Expression item = this.Parse(tokens, owner);
                nextItemAllowed = tokens.PopIfPresent(",");
                output.Add(item);
            }
            return output;
        }

        protected override AType MaybeParseCastPrefix(TokenStream tokens)
        {
            TokenStream.StreamState tss = tokens.RecordState();
            if (tokens.PopIfPresent("("))
            {
                AType output = this.parser.TypeParser.TryParse(tokens);
                if (output != null)
                {
                    if (tokens.PopIfPresent(")"))
                    {
                        if (!tokens.HasMore) return output; // let the next thing throw an error
                        if (output.Generics.Length > 0) return output;
                        switch (output.RootType)
                        {
                            case "int":
                            case "bool":
                            case "float":
                            case "string":
                            case "object":
                                return output;
                        }

                        switch (tokens.Peek().Type)
                        {
                            case TokenType.NUMBER:
                            case TokenType.STRING:
                            case TokenType.WORD:
                                return output;

                            case TokenType.KEYWORD:
                                break;

                            case TokenType.PUNCTUATION:
                                if (tokens.IsNext("("))
                                {
                                    return output;
                                }
                                break;
                        }
                    }
                }
            }
            tokens.RestoreState(tss);
            return null;
        }
    }
}
