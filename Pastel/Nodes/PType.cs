using System;
using System.Collections.Generic;
using System.Linq;

namespace Pastel.Nodes
{
    public class PType
    {
        public static readonly PType INT = new PType(null, "int");
        public static readonly PType CHAR = new PType(null, "char");
        public static readonly PType BOOL = new PType(null, "bool");
        public static readonly PType STRING = new PType(null, "string");
        public static readonly PType DOUBLE = new PType(null, "double");
        public static readonly PType VOID = new PType(null, "void");

        public Token FirstToken { get; set; }
        public string RootValue { get; set; }
        public PType[] Generics { get; set; }

        private static readonly PType[] EMPTY_GENERICS = new PType[0];

        public PType(Token firstToken, string value) : this(firstToken, value, null) { }

        public PType(Token firstToken, string value, IList<PType> generics)
        {
            this.FirstToken = firstToken;
            this.RootValue = value;
            this.Generics = generics == null ? EMPTY_GENERICS : generics.ToArray();
        }
        
        public static PType Parse(TokenStream tokens)
        {
            Token firstToken = tokens.Peek();
            PType type = ParseImpl(tokens);
            if (type == null)
            {
                throw new ParserException(firstToken, "Expected a type here.");
            }
            return type;
        }

        public static PType TryParse(TokenStream tokens)
        {
            int index = tokens.SnapshotState();
            PType type = ParseImpl(tokens);
            if (type == null)
            {
                tokens.RevertState(index);
            }
            return type;
        }

        private static PType ParseImpl(TokenStream tokens)
        {
            Token token = tokens.Pop();
            switch (token.Value)
            {
                case "int":
                case "char":
                case "double":
                case "bool":
                case "void":
                case "string":
                case "object":
                    return new PType(token, token.Value);
                default:
                    if (!PastelParser.IsValidName(token.Value))
                    {
                        return null;
                    }
                    break;
            }
            
            int tokenIndex = tokens.SnapshotState();
            bool isError = false;
            if (tokens.PopIfPresent("<"))
            {
                List<PType> generics = new List<PType>();
                while (!tokens.PopIfPresent(">"))
                {
                    if (generics.Count > 0)
                    {
                        if (!tokens.PopIfPresent(","))
                        {
                            isError = true;
                            break;
                        }
                    }

                    PType generic = ParseImpl(tokens);
                    if (generic == null) return null;

                    generics.Add(generic);
                }
                if (!isError)
                {
                    return new PType(token, token.Value, generics);
                }

                // If there was an error while parsing generics, then this may still be a valid type.
                tokens.RevertState(tokenIndex);
                return new PType(token, token.Value);
            }
            else
            {
                return new PType(token, token.Value);
            }
        }
    }
}
