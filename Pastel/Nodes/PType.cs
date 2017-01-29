using System;
using System.Collections.Generic;
using System.Linq;

namespace Pastel.Nodes
{
    class PType
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

        private static PType PopArray(PType root, TokenStream tokens)
        {
            while (tokens.IsNext("[") && tokens.FlatPeekAhead(1) == "]")
            {
                tokens.Pop();
                tokens.Pop();
                root = new PType(root.FirstToken, "Array", new List<PType>() { root });
            }
            return root;
        }

        public static PType Parse(TokenStream tokens)
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
                    return PopArray(new PType(token, token.Value), tokens);
            }

            if (tokens.PopIfPresent("<"))
            {
                List<PType> generics = new List<PType>();
                generics.Add(PType.Parse(tokens));
                while (tokens.PopIfPresent(","))
                {
                    generics.Add(PType.Parse(tokens));
                }
                tokens.PopExpectedOrPartial(">");
                return PopArray(new PType(token, token.Value, generics), tokens);
            }
            else
            {
                return PopArray(new PType(token, token.Value), tokens);
            }
        }
    }
}
