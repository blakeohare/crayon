using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon.Pastel.Nodes
{
    class PType
    {
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
            Token token = tokens.Pop();
            switch (token.Value)
            {
                case "int":
                case "double":
                case "bool":
                case "void":
                case "string":
                    return new PType(token, token.Value);
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
                return new PType(token, token.Value, generics);
            }
            else
            {
                return new PType(token, token.Value);
            }
        }
    }
}
