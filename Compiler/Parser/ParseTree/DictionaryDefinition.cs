using Common;
using Parser.Resolver;
using System.Collections.Generic;
using System.Linq;

namespace Parser.ParseTree
{
    public class DictionaryDefinition : Expression
    {
        public AType KeyType { get; private set; }
        public AType ValueType { get; private set; }
        public ResolvedType ResolvedKeyType { get; private set; }
        public ResolvedType ResolvedValueType { get; private set; }
        public Expression[] Keys { get; private set; }
        public Expression[] Values { get; private set; }

        public DictionaryDefinition(Token firstToken, AType keyType, AType valueType, IList<Expression> keys, IList<Expression> values, Node owner)
            : base(firstToken, owner)
        {
            this.KeyType = keyType;
            this.ValueType = valueType;
            this.Keys = keys.ToArray();
            this.Values = values.ToArray();
        }

        internal override IEnumerable<Expression> Descendants
        {
            get
            {
                int length = this.Keys.Length;
                if (length == 0) return Expression.NO_DESCENDANTS;
                List<Expression> output = new List<Expression>();
                for (int i = 0; i < length; ++i)
                {
                    output.Add(this.Keys[i]);
                    output.Add(this.Values[i]);
                }
                return output;
            }
        }

        internal override Expression Resolve(ParserContext parser)
        {

            HashSet<int> intKeyDupeCheck = new HashSet<int>();
            HashSet<string> stringKeyDupeCheck = new HashSet<string>();
            bool dictHasStringKeys = false;

            // Iterate through KVP in parallel so that errors will get reported in the preferred order.
            for (int i = 0; i < this.Keys.Length; ++i)
            {
                Expression keyExpr = this.Keys[i].Resolve(parser);
                IConstantValue keyConst = keyExpr as IConstantValue;
                bool keyIsString = keyConst is StringConstant;
                bool keyIsInteger = keyConst is IntegerConstant;
                if (!keyIsInteger && !keyIsString) throw new ParserException(keyExpr.FirstToken, "Only string and integer constants can be used as dictionary keys in inline definitions.");

                if (i == 0)
                {
                    dictHasStringKeys = keyIsString;
                }
                else if (dictHasStringKeys != keyIsString)
                {
                    throw new ParserException(keyExpr.FirstToken, "Cannot mix types of dictionary keys.");
                }

                bool collision = false;
                if (keyIsString)
                {
                    string keyValue = ((StringConstant)keyConst).Value;
                    collision = stringKeyDupeCheck.Contains(keyValue);
                    stringKeyDupeCheck.Add(keyValue);
                }
                else
                {
                    int keyValue = ((IntegerConstant)keyConst).Value;
                    collision = intKeyDupeCheck.Contains(keyValue);
                    intKeyDupeCheck.Add(keyValue);
                }

                this.Keys[i] = keyExpr;
                this.Values[i] = this.Values[i].Resolve(parser);
            }
            return this;
        }

        internal override void ResolveVariableOrigins(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase)
        {
            if ((phase & VariableIdAllocPhase.ALLOC) != 0)
            {
                // Iterate through KVP in parallel so that errors will get reported in the preferred order.
                for (int i = 0; i < this.Keys.Length; ++i)
                {
                    this.Keys[i].ResolveVariableOrigins(parser, varIds, phase);
                    this.Values[i].ResolveVariableOrigins(parser, varIds, phase);
                }
            }
        }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            this.BatchExpressionEntityNameResolver(parser, this.Keys);
            this.BatchExpressionEntityNameResolver(parser, this.Values);
            return this;
        }

        internal override Expression ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            int length = this.Keys.Length;
            this.ResolvedKeyType = typeResolver.ResolveType(this.KeyType);
            this.ResolvedValueType = typeResolver.ResolveType(this.ValueType);
            for (int i = 0; i < length; ++i)
            {
                this.Keys[i] = this.Keys[i].ResolveTypes(parser, typeResolver);
                this.Values[i] = this.Values[i].ResolveTypes(parser, typeResolver);

                if (!this.Keys[i].ResolvedType.CanAssignToA(this.ResolvedKeyType))
                    throw new ParserException(this.Keys[i], "This key is the incorrect type.");
                if (!this.Values[i].ResolvedType.CanAssignToA(this.ResolvedValueType))
                    throw new ParserException(this.Values[i], "This value is the incorrect type.");
            }

            if (this.ResolvedKeyType == ResolvedType.ANY || this.ResolvedValueType == ResolvedType.ANY)
            {
                this.ResolvedType = ResolvedType.ANY;
            }
            else
            {
                this.ResolvedType = ResolvedType.GetDictionaryType(this.ResolvedKeyType, this.ResolvedValueType);
            }

            return this;
        }
    }
}
