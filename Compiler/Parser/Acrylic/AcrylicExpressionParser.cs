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

            switch (className.RootType)
            {
                case "[":
                    throw new ParserException(newToken, "Arrays are not supported yet.");

                case "List":
                    tokens.PopExpected("(");
                    tokens.PopExpected(")");
                    List<Expression> items;
                    if (tokens.IsNext("{"))
                    {
                        items = this.ParseArrayDeclarationItems(tokens, owner);
                    } else
                    {
                        items = new List<Expression>();
                    }
                    return new ListDefinition(newToken, items, className.Generics[0], owner);

                case "Dictionary":
                    tokens.PopExpected("(");
                    tokens.PopExpected(")");
                    List<Expression> dictionaryKeys = new List<Expression>();
                    List<Expression> dictionaryValues = new List<Expression>();
                    if (tokens.IsNext("{"))
                    {
                        this.ParseDictionaryInlineItems(tokens, dictionaryKeys, dictionaryValues, owner);
                    }
                    return new DictionaryDefinition(
                        newToken,
                        className.Generics[0],
                        className.Generics[1],
                        dictionaryKeys,
                        dictionaryValues,
                        owner);

                default:
                    break;
            }

            IList<Expression> args = this.ParseArgumentList(tokens, owner);

            return new Instantiate(newToken, className.FirstToken, className.RootType, className.Generics, args, owner);
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

        private void ParseDictionaryInlineItems(TokenStream tokens, List<Expression> keysOut, List<Expression> valuesOut, Node owner)
        {
            tokens.PopExpected("{");
            bool nextAllowed = true;
            while (!tokens.PopIfPresent("}"))
            {
                if (!nextAllowed) tokens.PopExpected("}"); // crashes intentionally
                Expression key = this.Parse(tokens, owner);
                tokens.PopExpected(":");
                Expression value = this.Parse(tokens, owner);
                nextAllowed = tokens.PopIfPresent(",");
                keysOut.Add(key);
                valuesOut.Add(value);
            }
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
                        Token nextToken = tokens.Peek();
                        switch (nextToken.Type)
                        {
                            case TokenType.NUMBER:
                            case TokenType.STRING:
                            case TokenType.WORD:
                                return output;

                            case TokenType.KEYWORD:
                                switch (nextToken.Value)
                                {
                                    case "this":
                                    case "base":
                                        return output;
                                }
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
