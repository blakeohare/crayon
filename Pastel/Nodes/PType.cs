using System;
using System.Collections.Generic;
using System.Linq;

namespace Pastel.Nodes
{
    public class PType
    {
        // THIS MUST GO FIRST
        private static readonly PType[] EMPTY_GENERICS = new PType[0];

        public static readonly PType INT = new PType(null, "int");
        public static readonly PType CHAR = new PType(null, "char");
        public static readonly PType BOOL = new PType(null, "bool");
        public static readonly PType STRING = new PType(null, "string");
        public static readonly PType DOUBLE = new PType(null, "double");
        public static readonly PType VOID = new PType(null, "void");
        public static readonly PType NULL = new PType(null, "null");

        public Token FirstToken { get; set; }
        public string RootValue { get; set; }
        public PType[] Generics { get; set; }

        public bool IsNullable { get; set; }
        
        public PType(Token firstToken, string value, params PType[] generics) : this(firstToken, value, new List<PType>(generics)) { }
        public PType(Token firstToken, string value, List<PType> generics)
        {
            this.FirstToken = firstToken;
            this.RootValue = value;
            this.Generics = generics == null ? EMPTY_GENERICS : generics.ToArray();
            this.IsNullable = value == "string" || value == "object" || (value[0] >= 'A' && value[0] <= 'Z');
        }

        public static bool CheckAssignment(PType targetType, PType value)
        {
            if (targetType.RootValue == "void") return false;
            return CheckReturnType(targetType, value);
        }

        public static bool CheckReturnType(PType returnType, PType value)
        {
            if (returnType.IsIdentical(value)) return true;
            if (returnType.RootValue == "object") return true;
            if (returnType.RootValue == "void") return false;
            if (value.RootValue == "null")
            {
                if (returnType.RootValue == "string") return true;
                if (returnType.Generics.Length > 0) return true;

                // is struct?
                char c = returnType.RootValue[0];
                if (c >= 'A' && c <= 'Z') return true;
            }
            return false;
        }

        private bool IsParentOf(PType moreSpecificTypeOrSame)
        {
            if (moreSpecificTypeOrSame == this) return true;
            if (this.RootValue == "object") return true;
            if (this.Generics.Length == 0)
            {
                // why no treatment of int as a subtype of double? because there needs to be an explicit type conversion
                // for languages that aren't strongly typed and won't auto-convert.
                return this.RootValue == moreSpecificTypeOrSame.RootValue;
            }

            // All that's left are Arrays, Lists, and Dictionaries, which must match exactly.
            return this.IsIdentical(moreSpecificTypeOrSame);
        }

        public bool IsIdentical(PType other)
        {
            if (this.Generics.Length != other.Generics.Length) return false;
            if (this.RootValue != other.RootValue) return false;
            for (int i = this.Generics.Length - 1; i >= 0; --i)
            {
                if (!this.Generics[i].IsIdentical(other.Generics[i]))
                {
                    return false;
                }
            }
            return true;
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

        public override string ToString()
        {
            // only used for debugging and errors, so string concatenation is fine.
            string output = this.RootValue;
            if (this.Generics.Length > 0)
            {
                output += "<";
                for (int i = 0; i < this.Generics.Length; ++i)
                {
                    if (i > 0) output += ", ";
                    output += this.Generics[i].ToString();
                }
                output += ">";
            }
            return output;
        }
    }
}
