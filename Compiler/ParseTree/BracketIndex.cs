using Build;
using System.Collections.Generic;

namespace Crayon.ParseTree
{
    internal class BracketIndex : Expression
    {
        internal override Expression PastelResolve(Parser parser)
        {
            throw new System.NotImplementedException();
        }

        public override bool CanAssignTo { get { return true; } }

        public Expression Root { get; set; }
        public Token BracketToken { get; private set; }
        public Expression Index { get; set; }

        public BracketIndex(Expression root, Token bracketToken, Expression index, TopLevelConstruct owner)
            : base(root.FirstToken, owner)
        {
            this.Root = root;
            this.BracketToken = bracketToken;
            this.Index = index;
        }

        internal override Expression Resolve(Parser parser)
        {
            this.Root = this.Root.Resolve(parser);
            this.Index = this.Index.Resolve(parser);

            if (this.Root is CompileTimeDictionary)
            {
                // Swap out this bracketed expression with a fixed constant.
                CompileTimeDictionary root = (CompileTimeDictionary)this.Root;

                if (root.Type == "var")
                {
                    if (this.Index is StringConstant)
                    {
                        string index = ((StringConstant)this.Index).Value;

                        if (parser.BuildContext.BuildVariableLookup.ContainsKey(index))
                        {
                            BuildContext.BuildVarCanonicalized buildVar = parser.BuildContext.BuildVariableLookup[index];
                            switch (buildVar.Type)
                            {
                                case BuildContext.VarType.INT: return new IntegerConstant(this.FirstToken, buildVar.IntValue, this.Owner);
                                case BuildContext.VarType.FLOAT: return new FloatConstant(this.FirstToken, buildVar.FloatValue, this.Owner);
                                case BuildContext.VarType.STRING: return new StringConstant(this.FirstToken, buildVar.StringValue, this.Owner);
                                case BuildContext.VarType.BOOLEAN: return new BooleanConstant(this.FirstToken, buildVar.BoolValue, this.Owner);
                                default:
                                    throw new System.Exception("This should not happen."); // invalid types filtered during build context construction.

                            }
                        }
                        else
                        {
                            return new NullConstant(this.FirstToken, this.Owner);
                            // TODO: strict mode that enforces this. There are two ways this could be done:
                            // 1) $var['foo'] vs $optional['foo']
                            // 2) <strictvars>true</strictvars>
                            // I kind of prefer #1.

                            // throw new ParserException(this.Index.FirstToken, "The build variable with id '" + index + "' is not defined for this target.");
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

        internal override Expression ResolveNames(Parser parser)
        {
            this.Root = this.Root.ResolveNames(parser);
            this.Index = this.Index.ResolveNames(parser);
            return this;
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars)
        {
            this.Root.GetAllVariablesReferenced(vars);
            this.Index.GetAllVariablesReferenced(vars);
        }

        internal override void PerformLocalIdAllocation(Parser parser, VariableIdAllocator varIds, VariableIdAllocPhase phase)
        {
            if ((phase & VariableIdAllocPhase.ALLOC) != 0)
            {
                this.Root.PerformLocalIdAllocation(parser, varIds, phase);
                this.Index.PerformLocalIdAllocation(parser, varIds, phase);
            }
        }
    }
}
