using Builder.ParseTree;
using System.Collections.Generic;
using System.Linq;

namespace Builder.Resolver
{
    internal class TypeResolver
    {
        private readonly Node owner;
        private readonly TypeContext typeContext;

        public TypeResolver(TypeContext tc, Node owner)
        {
            this.typeContext = tc;
            this.owner = owner;
        }

        public ResolvedType[] ResolveTypes(IList<AType> types)
        {
            return types.Select(t => this.ResolveType(t)).ToArray();
        }

        public ResolvedType ResolveType(AType type)
        {
            if (type.IsAnyType) return this.typeContext.ANY;

            switch (type.RootType)
            {
                case "void": return this.typeContext.VOID;

                case "int": return this.typeContext.INTEGER;
                case "bool": return this.typeContext.BOOLEAN;
                case "float": return this.typeContext.FLOAT;
                case "string": return this.typeContext.STRING;
                case "object": return this.typeContext.OBJECT;

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
                    return ResolvedType.GetInstanceType(this.typeContext, classDef);
            }
        }

        public ResolvedType FindCommonAncestor(ResolvedType typeA, ResolvedType typeB)
        {
            if (typeA == this.typeContext.ANY || typeB == this.typeContext.ANY) return this.typeContext.ANY;
            if (typeA == typeB) return typeA;

            if ((typeA == this.typeContext.INTEGER && typeB == this.typeContext.FLOAT) ||
                (typeA == this.typeContext.FLOAT && typeB == this.typeContext.INTEGER))
            {
                return this.typeContext.FLOAT;
            }

            if (typeA == this.typeContext.NULL || typeB == this.typeContext.NULL)
            {
                ResolvedType nonNull = typeA == this.typeContext.NULL ? typeB : typeA;
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
                        return this.typeContext.OBJECT;
                }
            }

            if (typeA.Category != typeB.Category)
            {
                return this.typeContext.OBJECT;
            }

            switch (typeA.Category)
            {
                case ResolvedTypeCategory.LIST:
                case ResolvedTypeCategory.DICTIONARY:
                    return this.typeContext.OBJECT;

                case ResolvedTypeCategory.INSTANCE:
                    ClassDefinition c1 = typeA.ClassTypeOrReference;
                    ClassDefinition c2 = typeB.ClassTypeOrReference;
                    ClassDefinition commonParent = c1.GetCommonAncestor(c2);
                    if (commonParent == null)
                    {
                        return this.typeContext.OBJECT;
                    }
                    return ResolvedType.GetInstanceType(this.typeContext, commonParent);

                default:
                    throw new System.NotImplementedException();
            }
        }
    }
}
