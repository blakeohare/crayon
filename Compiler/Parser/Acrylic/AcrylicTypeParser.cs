using System.Collections.Generic;

namespace Parser.Acrylic
{
    internal class AcrylicTypeParser : AbstractTypeParser
    {
        private AType[] EMPTY_TYPE_LIST = new AType[0];
        private AType[] SIMPLE_TYPE_LIST = new AType[1];
        private Token[] SIMPLE_TOKEN_LIST = new Token[1];

        public override AType TryParse(TokenStream tokens)
        {
            TokenStream.StreamState startIndex = tokens.RecordState();
            AType type = this.ParseImpl(tokens);
            if (type == null)
            {
                tokens.RestoreState(startIndex);
            }
            return type;
        }

        public override AType Parse(TokenStream tokens)
        {
            tokens.EnsureNotEof();
            Token throwToken = tokens.Peek();
            AType type = this.ParseImpl(tokens);
            if (type == null) throw new ParserException(throwToken, "Expected type.");
            return type;
        }

        private AType ParseImpl(TokenStream tokens)
        {
            AType rootType = this.ParseImplRoot(tokens);
            while (rootType != null && tokens.AreNext("[", "]"))
            {
                Token arrayToken = tokens.Pop();
                tokens.Pop();
                Token firstToken = rootType.FirstToken;
                SIMPLE_TOKEN_LIST[0] = arrayToken;
                SIMPLE_TYPE_LIST[0] = rootType;
                rootType = new AType(SIMPLE_TOKEN_LIST, SIMPLE_TYPE_LIST);
                rootType.FirstToken = firstToken;
            }
            return rootType;
        }

        private AType ParseImplRoot(TokenStream tokens)
        {
            switch (tokens.PeekValue())
            {
                // Don't even try to parse generics or namespaces on primitive types.
                case "int":
                case "bool":
                case "double":
                case "string":
                    SIMPLE_TOKEN_LIST[0] = tokens.Pop();
                    return new AType(SIMPLE_TOKEN_LIST, EMPTY_TYPE_LIST);
            }

            Token firstToken = tokens.PopIfWord();
            if (firstToken != null)
            {
                List<Token> rootType = new List<Token>() { firstToken };
                while (tokens.PopIfPresent("."))
                {
                    TokenStream.StreamState tss = tokens.RecordState();
                    Token next = tokens.PopIfWord();
                    if (next == null)
                    {
                        tokens.RestoreState(tss);
                        break;
                    }
                    else
                    {
                        rootType.Add(next);
                    }
                }
                IList<AType> generics = EMPTY_TYPE_LIST;
                if (tokens.IsNext("<"))
                {
                    TokenStream.StreamState tss = tokens.RecordState();
                    generics = this.ParseGenerics(tokens);
                    if (generics.Count == 0) tokens.RestoreState(tss);
                }
                return new AType(rootType, generics);
            }
            return null;
        }

        private IList<AType> ParseGenerics(TokenStream tokens)
        {
            tokens.Pop(); // '<'
            AType firstType = this.ParseImpl(tokens);
            if (firstType == null) return EMPTY_TYPE_LIST;
            List<AType> generics = new List<AType>() { firstType };
            while (tokens.PopIfPresent(","))
            {
                AType next = this.ParseImpl(tokens);
                if (next == null) return EMPTY_TYPE_LIST;
                generics.Add(next);
            }
            if (!tokens.PopIfPresent(">")) return EMPTY_TYPE_LIST;
            return generics;
        }
    }
}
