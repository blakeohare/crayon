using System.Collections.Generic;
using System.Linq;
using Parser.ParseTree;

namespace Parser
{
    internal enum ResolvedTypeCategory
    {
        ANY,
        VOID,
        NULL,
        NULLABLE,
        OBJECT,

        BOOLEAN,
        INTEGER,
        FLOAT,
        STRING,
        LIST, // TODO: split this into an arrya
        DICTIONARY,
        CLASS_DEFINITION,
        INSTANCE,
        FUNCTION_POINTER,
    }

    internal class TypeContext
    {
        // A static reference to the most recently active TypeContext
        // TODO: figure out how to remove this. Currently resolved types are set on all constant values in their constructors
        // but shouldn't be set until the ResolveType resolution phase. However, OpChain consolidation occurs in the Resolve
        // phase which comes before Type Resolution.
        public static TypeContext HACK_REF { get; set; }

        private int idAlloc = 1;
        public int GetNextId() { return this.idAlloc++; }

        public TypeContext()
        {
            HACK_REF = this;

            this.VOID = new ResolvedType(this, ResolvedTypeCategory.VOID);
            this.ANY = new ResolvedType(this, ResolvedTypeCategory.ANY);
            this.BOOLEAN = new ResolvedType(this, ResolvedTypeCategory.BOOLEAN);
            this.INTEGER = new ResolvedType(this, ResolvedTypeCategory.INTEGER);
            this.FLOAT = new ResolvedType(this, ResolvedTypeCategory.FLOAT);
            this.STRING = new ResolvedType(this, ResolvedTypeCategory.STRING);
            this.NULL = new ResolvedType(this, ResolvedTypeCategory.NULL);
            this.OBJECT = new ResolvedType(this, ResolvedTypeCategory.OBJECT);
        }

        public readonly ResolvedType VOID;
        public readonly ResolvedType ANY;
        public readonly ResolvedType BOOLEAN;
        public readonly ResolvedType INTEGER;
        public readonly ResolvedType FLOAT;
        public readonly ResolvedType STRING;
        public readonly ResolvedType NULL;
        public readonly ResolvedType OBJECT;

        public readonly Dictionary<int, ResolvedType> ArrayTypes = new Dictionary<int, ResolvedType>();
        public readonly Dictionary<ClassDefinition, ResolvedType> InstanceTypes = new Dictionary<ClassDefinition, ResolvedType>();
        public readonly Dictionary<ClassDefinition, ResolvedType> ClassRefTypes = new Dictionary<ClassDefinition, ResolvedType>();
        public readonly Dictionary<int, ResolvedType> NullableTypes = new Dictionary<int, ResolvedType>();
        public readonly Dictionary<int, Dictionary<int, ResolvedType>> DictionaryTypes = new Dictionary<int, Dictionary<int, ResolvedType>>();
        public readonly Dictionary<FunctionDefinition, ResolvedType> FuncTypesByRef = new Dictionary<FunctionDefinition, ResolvedType>();
        public readonly Dictionary<string, ResolvedType> FuncTypes = new Dictionary<string, ResolvedType>();
    }

    internal class ResolvedType
    {
        public override string ToString()
        {
            return "Resolved Type: " + this.Category.ToString();
        }

        public string ToUserString(Parser.Localization.Locale locale)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            this.ToUserStringImpl(sb, locale);
            return sb.ToString();
        }

        private void ToUserStringImpl(System.Text.StringBuilder sb, Parser.Localization.Locale locale)
        {
            switch (this.Category)
            {
                case ResolvedTypeCategory.BOOLEAN: sb.Append("boolean"); return;
                case ResolvedTypeCategory.FLOAT: sb.Append("float"); return;
                case ResolvedTypeCategory.INTEGER: sb.Append("integer"); return;
                case ResolvedTypeCategory.OBJECT: sb.Append("object"); return;
                case ResolvedTypeCategory.NULL: sb.Append("null"); return;
                case ResolvedTypeCategory.STRING: sb.Append("string"); return;
                case ResolvedTypeCategory.LIST:
                    sb.Append("List<");
                    this.Generics[0].ToUserStringImpl(sb, locale);
                    sb.Append('>');
                    return;
                case ResolvedTypeCategory.DICTIONARY:
                    sb.Append("Dictionary<");
                    this.Generics[0].ToUserStringImpl(sb, locale);
                    sb.Append(", ");
                    this.Generics[1].ToUserStringImpl(sb, locale);
                    sb.Append('>');
                    return;
                case ResolvedTypeCategory.VOID: sb.Append("void"); return;
                case ResolvedTypeCategory.NULLABLE:
                    this.Generics[0].ToUserStringImpl(sb, locale);
                    sb.Append('?');
                    return;
                case ResolvedTypeCategory.INSTANCE:
                    sb.Append(this.ClassTypeOrReference.GetFullyQualifiedLocalizedName(locale));
                    return;
                case ResolvedTypeCategory.CLASS_DEFINITION:
                    sb.Append("Class<");
                    sb.Append(this.ClassTypeOrReference.GetFullyQualifiedLocalizedName(locale));
                    sb.Append('>');
                    return;
                case ResolvedTypeCategory.ANY:
                    sb.Append("[ANY]");
                    return;
                case ResolvedTypeCategory.FUNCTION_POINTER:
                    sb.Append("Func<");
                    for (int i = 0; i < this.FunctionArgs.Length; ++i)
                    {
                        this.FunctionArgs[i].ToUserStringImpl(sb, locale);
                        sb.Append(", ");
                    }
                    this.FunctionReturnType.ToUserStringImpl(sb, locale);
                    sb.Append('>');
                    return;
                default:
#if DEBUG
                    throw new System.NotImplementedException(this.ToString());
#else
                    sb.Append("<???>");
                    return;
#endif
            }
        }

        public ResolvedTypeCategory Category { get; private set; }
        public ClassDefinition ClassTypeOrReference { get; private set; }
        public ResolvedType[] Generics { get; private set; }
        public ResolvedType ListItemType { get { return this.Generics[0]; } }
        public ResolvedType DictionaryKeyType { get { return this.Generics[0]; } }
        public ResolvedType DictionaryValueType { get { return this.Generics[1]; } }
        public ResolvedType FunctionReturnType { get; set; }
        public ResolvedType[] FunctionArgs { get { return this.Generics; } }
        public ResolvedType NullableType { get { return this.Generics[0]; } }
        public int FunctionOptionalArgCount { get; private set; }

        private int id;
        private TypeContext ctx;
        public TypeContext TypeContext { get { return this.ctx; } }

        internal ResolvedType(TypeContext ctx, ResolvedTypeCategory category)
        {
            this.ctx = ctx;
            this.Category = category;
            this.id = ctx.GetNextId();
        }

        public static ResolvedType ListOrArrayOf(ResolvedType otherType)
        {
            TypeContext ctx = otherType.ctx;
            ResolvedType output;
            if (!ctx.ArrayTypes.TryGetValue(otherType.id, out output))
            {
                output = new ResolvedType(ctx, ResolvedTypeCategory.LIST);
                output.Generics = new ResolvedType[] { otherType };
                ctx.ArrayTypes[otherType.id] = output;
            }
            return output;
        }

        private static ResolvedType GetClassTypeImpl(TypeContext ctx, ResolvedTypeCategory cat, ClassDefinition cd)
        {
            ResolvedType output;
            Dictionary<ClassDefinition, ResolvedType> lookup = cat == ResolvedTypeCategory.CLASS_DEFINITION
                ? ctx.ClassRefTypes
                : ctx.InstanceTypes;
            if (!lookup.TryGetValue(cd, out output))
            {
                output = new ResolvedType(ctx, cat) { ClassTypeOrReference = cd };
                lookup[cd] = output;
            }
            return output;
        }
        public static ResolvedType GetInstanceType(TypeContext ctx, ClassDefinition cd)
        {
            return GetClassTypeImpl(ctx, ResolvedTypeCategory.INSTANCE, cd);
        }
        public static ResolvedType GetClassRefType(TypeContext ctx, ClassDefinition cd)
        {
            return GetClassTypeImpl(ctx, ResolvedTypeCategory.CLASS_DEFINITION, cd);
        }

        public static ResolvedType GetNullableType(ResolvedType type)
        {
            ResolvedType output;
            if (!type.ctx.NullableTypes.TryGetValue(type.id, out output))
            {
                output = new ResolvedType(type.ctx, ResolvedTypeCategory.NULLABLE) { Generics = new ResolvedType[] { type } };
                type.ctx.NullableTypes[type.id] = output;
            }
            return output;
        }

        public static ResolvedType GetDictionaryType(ResolvedType key, ResolvedType value)
        {
            Dictionary<int, ResolvedType> lookup;
            if (!key.ctx.DictionaryTypes.TryGetValue(key.id, out lookup))
            {
                lookup = new Dictionary<int, ResolvedType>();
                key.ctx.DictionaryTypes[key.id] = lookup;
            }
            ResolvedType output;
            if (!lookup.TryGetValue(value.id, out output))
            {
                output = new ResolvedType(key.ctx, ResolvedTypeCategory.DICTIONARY)
                {
                    Generics = new ResolvedType[] { key, value },
                };
                lookup[value.id] = output;
            }
            return output;
        }

        public static ResolvedType GetFunctionType(FunctionDefinition func)
        {
            TypeContext ctx = func.ResolvedReturnType.TypeContext;
            if (!ctx.FuncTypesByRef.ContainsKey(func))
            {
                ResolvedType type = GetFunctionType(func.ResolvedReturnType, func.ResolvedArgTypes, FunctionCall.CountOptionalArgs(func.DefaultValues));
                ctx.FuncTypesByRef[func] = type;
            }
            return ctx.FuncTypesByRef[func];
        }

        public static ResolvedType GetFunctionType(ResolvedType returnType, IList<ResolvedType> args, int optionalCount)
        {
            TypeContext ctx = returnType.TypeContext;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append('f');
            sb.Append(optionalCount);
            sb.Append(',');
            sb.Append(returnType.id);
            for (int i = 0; i < args.Count; ++i)
            {
                sb.Append(',');
                sb.Append(args[i].id);
            }
            string key = sb.ToString();
            ResolvedType output;
            if (!ctx.FuncTypes.TryGetValue(key, out output))
            {
                output = new ResolvedType(returnType.ctx, ResolvedTypeCategory.FUNCTION_POINTER);
                output.FunctionReturnType = returnType;
                output.Generics = args.ToArray();
                output.FunctionOptionalArgCount = optionalCount;
                ctx.FuncTypes[key] = output;
            }
            return output;
        }

        public void EnsureCanAssignToA(Token throwToken, ResolvedType targetType)
        {
            if (!CanAssignToA(targetType))
            {
                Parser.Localization.Locale en = Parser.Localization.Locale.Get("en");

                // TODO: use the correct locale
                string msg = "Cannot assign a value of type '";
                msg += this.ToUserString(en);
                msg += "' to '";
                msg += targetType.ToUserString(en);
                msg += "'";

                throw new ParserException(throwToken, msg);
            }
        }

        public bool CanAssignToA(ResolvedType targetType)
        {
            if (this.Category == ResolvedTypeCategory.ANY) return true;
            ResolvedTypeCategory targetCategory = targetType.Category;
            if (targetCategory == ResolvedTypeCategory.ANY) return true;
            if (targetCategory == ResolvedTypeCategory.OBJECT) return true;
            if (this.Category == ResolvedTypeCategory.NULL)
            {
                switch (targetCategory)
                {
                    case ResolvedTypeCategory.INSTANCE:
                    case ResolvedTypeCategory.CLASS_DEFINITION:
                    case ResolvedTypeCategory.FUNCTION_POINTER:
                    case ResolvedTypeCategory.STRING:
                    case ResolvedTypeCategory.LIST:
                    case ResolvedTypeCategory.DICTIONARY:
                        return true;
                    default:
                        return false;
                }
            }

            if (this.Category == ResolvedTypeCategory.VOID) return false;

            if (this.Category == ResolvedTypeCategory.INTEGER &&
                targetCategory == ResolvedTypeCategory.FLOAT)
            {
                return true;
            }

            if (this.Category != targetCategory) return false;

            if (targetCategory == ResolvedTypeCategory.INSTANCE)
            {
                ClassDefinition targetClass = targetType.ClassTypeOrReference;
                ClassDefinition baseClassWalker = this.ClassTypeOrReference;
                while (baseClassWalker != null)
                {
                    if (baseClassWalker == targetClass) return true;
                    baseClassWalker = baseClassWalker.BaseClass;
                }
                return false;
            }

            if (this.Category == ResolvedTypeCategory.LIST || this.Category == ResolvedTypeCategory.DICTIONARY)
            {
                TypeContext ctx = targetType.ctx;
                for (int i = 0; i < this.Generics.Length; ++i)
                {
                    if (this.Generics[i] == targetType.Generics[i]) { }
                    else if (this.Generics[i] == ctx.ANY && targetType.Generics[i] == ctx.OBJECT) { }
                    else { return false; }
                }
                return true;
            }

            return true;
        }

        public int[] GetEncoding()
        {
            List<int> output = new List<int>();
            this.BuildEncoding(output);
            return output.ToArray();
        }

        public void BuildEncoding(List<int> buffer)
        {
            switch (this.Category)
            {
                case ResolvedTypeCategory.OBJECT:
                case ResolvedTypeCategory.ANY:
                    buffer.Add(0);
                    break;

                case ResolvedTypeCategory.BOOLEAN: buffer.Add((int)Types.BOOLEAN); break;
                case ResolvedTypeCategory.INTEGER: buffer.Add((int)Types.INTEGER); break;
                case ResolvedTypeCategory.FLOAT: buffer.Add((int)Types.FLOAT); break;
                case ResolvedTypeCategory.CLASS_DEFINITION: buffer.Add((int)Types.CLASS); break;
                case ResolvedTypeCategory.STRING: buffer.Add((int)Types.STRING); break;

                case ResolvedTypeCategory.LIST:
                    buffer.Add((int)Types.LIST);
                    this.ListItemType.BuildEncoding(buffer);
                    break;

                case ResolvedTypeCategory.DICTIONARY:
                    buffer.Add((int)Types.DICTIONARY);
                    this.DictionaryKeyType.BuildEncoding(buffer);
                    this.DictionaryValueType.BuildEncoding(buffer);
                    break;

                case ResolvedTypeCategory.INSTANCE:
                    buffer.Add((int)Types.INSTANCE);
                    buffer.Add(this.ClassTypeOrReference.ClassID);
                    break;

                case ResolvedTypeCategory.FUNCTION_POINTER:
                    buffer.Add((int)Types.FUNCTION);
                    buffer.Add(this.FunctionArgs.Length + 1);
                    buffer.Add(this.FunctionOptionalArgCount);
                    this.FunctionReturnType.BuildEncoding(buffer);
                    foreach (ResolvedType t in this.FunctionArgs)
                    {
                        t.BuildEncoding(buffer);
                    }
                    break;

                default:
                    throw new System.NotImplementedException();
            }
        }
    }
}
