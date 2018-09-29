using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parser.ParseTree;

namespace Parser
{
    public enum ResolvedTypeCategory
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

    public class ResolvedType
    {
        public override string ToString()
        {
            return "Resolved Type: " + this.Category.ToString();
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
        private static int idAlloc = 0;

        private ResolvedType(ResolvedTypeCategory category)
        {
            this.Category = category;
            this.id = idAlloc++;
        }

        public static readonly ResolvedType VOID = new ResolvedType(ResolvedTypeCategory.VOID);
        public static readonly ResolvedType ANY = new ResolvedType(ResolvedTypeCategory.ANY);
        public static readonly ResolvedType BOOLEAN = new ResolvedType(ResolvedTypeCategory.BOOLEAN);
        public static readonly ResolvedType INTEGER = new ResolvedType(ResolvedTypeCategory.INTEGER);
        public static readonly ResolvedType FLOAT = new ResolvedType(ResolvedTypeCategory.FLOAT);
        public static readonly ResolvedType STRING = new ResolvedType(ResolvedTypeCategory.STRING);
        public static readonly ResolvedType NULL = new ResolvedType(ResolvedTypeCategory.NULL);
        public static readonly ResolvedType OBJECT = new ResolvedType(ResolvedTypeCategory.OBJECT);

        private static readonly Dictionary<int, ResolvedType> arrayTypes = new Dictionary<int, ResolvedType>();
        public static ResolvedType ListOrArrayOf(ResolvedType otherType)
        {
            ResolvedType output;
            if (!arrayTypes.TryGetValue(otherType.id, out output))
            {
                output = new ResolvedType(ResolvedTypeCategory.LIST);
                output.Generics = new ResolvedType[] { otherType };
                arrayTypes[otherType.id] = output;
            }
            return output;
        }

        private static readonly Dictionary<ClassDefinition, ResolvedType> instanceTypes = new Dictionary<ClassDefinition, ResolvedType>();
        private static readonly Dictionary<ClassDefinition, ResolvedType> classRefTypes = new Dictionary<ClassDefinition, ResolvedType>();
        private static ResolvedType GetClassTypeImpl(ResolvedTypeCategory cat, ClassDefinition cd)
        {
            ResolvedType output;
            Dictionary<ClassDefinition, ResolvedType> lookup = cat == ResolvedTypeCategory.CLASS_DEFINITION
                ? classRefTypes
                : instanceTypes;
            if (!lookup.TryGetValue(cd, out output))
            {
                output = new ResolvedType(cat) { ClassTypeOrReference = cd };
                lookup[cd] = output;
            }
            return output;
        }
        public static ResolvedType GetInstanceType(ClassDefinition cd)
        {
            return GetClassTypeImpl(ResolvedTypeCategory.INSTANCE, cd);
        }
        public static ResolvedType GetClassRefType(ClassDefinition cd)
        {
            return GetClassTypeImpl(ResolvedTypeCategory.CLASS_DEFINITION, cd);
        }

        private static readonly Dictionary<int, ResolvedType> nullableTypes = new Dictionary<int, ResolvedType>();
        public static ResolvedType GetNullableType(ResolvedType type)
        {
            ResolvedType output;
            if (!nullableTypes.TryGetValue(type.id, out output))
            {
                output = new ResolvedType(ResolvedTypeCategory.NULLABLE) { Generics = new ResolvedType[] { type } };
                nullableTypes[type.id] = output;
            }
            return output;
        }

        private static readonly Dictionary<int, Dictionary<int, ResolvedType>> dictionaryTypes = new Dictionary<int, Dictionary<int, ResolvedType>>();
        public static ResolvedType GetDictionaryType(ResolvedType key, ResolvedType value)
        {
            Dictionary<int, ResolvedType> lookup;
            if (!dictionaryTypes.TryGetValue(key.id, out lookup))
            {
                lookup = new Dictionary<int, ResolvedType>();
                dictionaryTypes[key.id] = lookup;
            }
            ResolvedType output;
            if (!lookup.TryGetValue(value.id, out output))
            {
                output = new ResolvedType(ResolvedTypeCategory.DICTIONARY)
                {
                    Generics = new ResolvedType[] { key, value },
                };
                lookup[value.id] = output;
            }
            return output;
        }

        private static readonly Dictionary<FunctionDefinition, ResolvedType> funcTypesByRef = new Dictionary<FunctionDefinition, ResolvedType>();
        public static ResolvedType GetFunctionType(FunctionDefinition func)
        {
            if (!funcTypesByRef.ContainsKey(func))
            {
                ResolvedType type = GetFunctionType(func.ResolvedReturnType, func.ResolvedArgTypes, FunctionCall.CountOptionalArgs(func.DefaultValues));
                funcTypesByRef[func] = type;
            }
            return funcTypesByRef[func];
        }

        private static readonly Dictionary<string, ResolvedType> funcTypes = new Dictionary<string, ResolvedType>();
        public static ResolvedType GetFunctionType(ResolvedType returnType, IList<ResolvedType> args, int optionalCount)
        {
            StringBuilder sb = new StringBuilder();
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
            if (!funcTypes.TryGetValue(key, out output))
            {
                output = new ResolvedType(ResolvedTypeCategory.FUNCTION_POINTER);
                output.FunctionReturnType = returnType;
                output.Generics = args.ToArray();
                output.FunctionOptionalArgCount = optionalCount;
                funcTypes[key] = output;
            }
            return output;
        }

        public void EnsureCanAssignToA(Token throwToken, ResolvedType targetType)
        {
            if (!CanAssignToA(targetType))
            {
                // TODO: implement a formatted string method for ResolvedType. (ToString() is debug-centric)
                throw new ParserException(throwToken, "Cannot assign this type to this other type.");
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
