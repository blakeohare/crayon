using Parser.ParseTree;
using System.Collections.Generic;
using System.Linq;

namespace Parser.Resolver
{
    internal class TypeResolver
    {
        private readonly Node owner;

        public TypeResolver(Node owner)
        {
            this.owner = owner;
        }

        public ResolvedType[] ResolveTypes(IList<AType> types)
        {
            return types.Select(t => this.ResolveType(t)).ToArray();
        }

        public ResolvedType ResolveType(AType type)
        {
            if (type.IsAnyType) return ResolvedType.ANY;

            switch (type.RootType)
            {
                case "void": return ResolvedType.VOID;

                case "int": return ResolvedType.INTEGER;
                case "bool": return ResolvedType.BOOLEAN;
                case "float": return ResolvedType.FLOAT;
                case "string": return ResolvedType.STRING;

                case "[":
                case "List":
                    if (type.Generics.Length != 1) throw new ParserException(type.FirstToken, "A list type has 1 generic.");
                    return ResolvedType.ListOrArrayOf(ResolveType(type.Generics[0]));

                case "Dictionary":
                    if (type.Generics.Length != 2) throw new ParserException(type.FirstToken, "A dictionary type has 2 generics.");
                    ResolvedType keyType = ResolveType(type.Generics[0]);
                    ResolvedType valueType = ResolveType(type.Generics[1]);
                    switch (keyType.Category)
                    {
                        case ResolvedTypeCategory.INSTANCE:
                        case ResolvedTypeCategory.INTEGER:
                        case ResolvedTypeCategory.STRING:
                            // This is fine.
                            break;

                        default:
                            throw new ParserException(type.Generics[0].FirstToken, "This is not a valid key type for a dictionary.");
                    }
                    return ResolvedType.GetDictionaryType(keyType, valueType);

                default:
                    ClassDefinition classDef = this.owner.FileScope.DoClassLookup(this.owner, type.FirstToken, type.RootType);
                    return ResolvedType.GetInstanceType(classDef);
            }
        }
    }
}
