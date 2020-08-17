using Parser.Resolver;
using System.Collections.Generic;

namespace Parser.ParseTree
{
    internal class BracketIndex : Expression
    {
        public override bool CanAssignTo { get { return true; } }

        public Expression Root { get; set; }
        public Token BracketToken { get; private set; }
        public Expression Index { get; set; }

        public BracketIndex(Expression root, Token bracketToken, Expression index, Node owner)
            : base(root.FirstToken, owner)
        {
            this.Root = root;
            this.BracketToken = bracketToken;
            this.Index = index;
        }

        internal override IEnumerable<Expression> Descendants { get { return new Expression[] { this.Root, this.Index }; } }

        internal override Expression Resolve(ParserContext parser)
        {
            this.Root = this.Root.Resolve(parser);
            this.Index = this.Index.Resolve(parser);

            return this;
        }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            this.Root = this.Root.ResolveEntityNames(parser);
            this.Index = this.Index.ResolveEntityNames(parser);

            if (this.Root is CompileTimeDictionary)
            {
                // Swap out this bracketed expression with a fixed constant.
                CompileTimeDictionary root = (CompileTimeDictionary)this.Root;

                if (root.Type == "var")
                {
                    if (this.Index is StringConstant)
                    {
                        string varName = ((StringConstant)this.Index).Value;

                        if (parser.CompileRequest.HasCompileTimeValue(varName))
                        {
                            switch ((Types) parser.CompileRequest.GetCompileTimeValueType(varName))
                            {
                                case Types.BOOLEAN: return new BooleanConstant(this.FirstToken, parser.CompileRequest.GetCompileTimeBool(varName), this.Owner);
                                case Types.INTEGER: return new IntegerConstant(this.FirstToken, parser.CompileRequest.GetCompileTimeInt(varName), this.Owner);
                                case Types.FLOAT: return new FloatConstant(this.FirstToken, parser.CompileRequest.GetCompileTimeFloat(varName), this.Owner);
                                case Types.STRING: return new StringConstant(this.FirstToken, parser.CompileRequest.GetCompileTimeString(varName), this.Owner);
                                default: throw new System.Exception(); // this should not happen.
                            }
                        }
                        else
                        {
                            return new NullConstant(this.FirstToken, this.Owner);
                            // TODO: strict mode that enforces this. There are two ways this could be done:
                            // 1) $var['foo'] vs $optional['foo']
                            // 2) <strictvars>true</strictvars>
                            // I kind of prefer #1.

                            // throw new ParserException(this.Index, "The build variable with id '" + index + "' is not defined for this target.");
                        }
                    }
                }
                else
                {
                    throw new System.Exception("Unknown compile time dictionary type.");
                }
            }

            return this;
        }

        internal override Expression ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            this.Root = this.Root.ResolveTypes(parser, typeResolver);
            this.Index = this.Index.ResolveTypes(parser, typeResolver);
            ResolvedTypeCategory indexType = this.Index.ResolvedType.Category;

            switch (this.Root.ResolvedType.Category)
            {
                case ResolvedTypeCategory.ANY:
                    this.ResolvedType = ResolvedType.ANY;
                    switch (indexType)
                    {
                        case ResolvedTypeCategory.ANY:
                        case ResolvedTypeCategory.INTEGER:
                        case ResolvedTypeCategory.STRING:
                        case ResolvedTypeCategory.INSTANCE:
                            // these are possibly fine
                            break;
                        default:
                            throw new ParserException(this.Index, "Cannot use this value as an index.");
                    }
                    break;

                case ResolvedTypeCategory.LIST:
                    this.ResolvedType = this.Root.ResolvedType.ListItemType;
                    if (!this.Index.ResolvedType.CanAssignToA(ResolvedType.INTEGER))
                    {
                        throw new ParserException(this.Index, "Can only index into a list with an integer.");
                    }
                    break;

                case ResolvedTypeCategory.DICTIONARY:
                    this.ResolvedType = this.Root.ResolvedType.DictionaryValueType;
                    if (!this.Index.ResolvedType.CanAssignToA(this.Root.ResolvedType.DictionaryKeyType))
                    {
                        throw new ParserException(this.Index, "Cannot index into this dictionary with this type.");
                    }
                    break;

                case ResolvedTypeCategory.STRING:
                    this.ResolvedType = ResolvedType.STRING;
                    if (!this.Index.ResolvedType.CanAssignToA(ResolvedType.INTEGER))
                    {
                        throw new ParserException(this.Index, "Can only index into a string with an integer.");
                    }
                    break;

                case ResolvedTypeCategory.INSTANCE:
                    throw new ParserException(this.Index, "Cannot use brackets to index into an instance of " + this.Root.ResolvedType.ClassTypeOrReference.NameToken.Value + ".");

                default:
                    throw new ParserException(this.Root, "Cannot index into this kind of value.");
            }
            return this;
        }

        internal override void ResolveVariableOrigins(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase)
        {
            if ((phase & VariableIdAllocPhase.ALLOC) != 0)
            {
                this.Root.ResolveVariableOrigins(parser, varIds, phase);
                this.Index.ResolveVariableOrigins(parser, varIds, phase);
            }
        }
    }
}
