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

        public ResolvedType FindCommonAncestor(ResolvedType typeA, ResolvedType typeB)
        {
            if (typeA == ResolvedType.ANY || typeB == ResolvedType.ANY) return ResolvedType.ANY;
            if (typeA == typeB) return typeA;

            if ((typeA == ResolvedType.INTEGER && typeB == ResolvedType.FLOAT) ||
                (typeA == ResolvedType.FLOAT && typeB == ResolvedType.INTEGER))
            {
                return ResolvedType.FLOAT;
            }

            if (typeA == ResolvedType.NULL || typeB == ResolvedType.NULL)
            {
                ResolvedType nonNull = typeA == ResolvedType.NULL ? typeB : typeA;
                switch (nonNull.Category)
                {
                    case ResolvedTypeCategory.STRING:
                    case ResolvedTypeCategory.LIST:
                    case ResolvedTypeCategory.DICTIONARY:
                    case ResolvedTypeCategory.FUNCTION_POINTER:
                    case ResolvedTypeCategory.INSTANCE:
                    case ResolvedTypeCategory.CLASS_DEFINITION:
                        return nonNull;
                    default:
                        return ResolvedType.OBJECT;
                }
            }

            if (typeA.Category != typeB.Category)
            {
                return ResolvedType.OBJECT;
            }

            switch (typeA.Category)
            {
                case ResolvedTypeCategory.LIST:
                case ResolvedTypeCategory.DICTIONARY:
                    return ResolvedType.OBJECT;

                case ResolvedTypeCategory.INSTANCE:
                    ClassDefinition c1 = typeA.ClassTypeOrReference;
                    ClassDefinition c2 = typeB.ClassTypeOrReference;
                    ClassDefinition commonParent = c1.GetCommonAncestor(c2);
                    if (commonParent == null)
                    {
                        return ResolvedType.OBJECT;
                    }
                    return ResolvedType.GetInstanceType(commonParent);

                default:
                    throw new System.NotImplementedException();
            }
        }
    }
}
