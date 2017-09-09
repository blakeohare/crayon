using System.Collections.Generic;

namespace Crayon.ParseTree
{
    internal class ConstStatement : TopLevelConstruct
    {
        internal override Executable PastelResolve(Parser parser)
        {
            this.Expression = this.Expression.PastelResolve(parser);
            return this;
        }

        public Expression Expression { get; set; }
        public Token NameToken { get; private set; }
        public string Name { get; private set; }

        public ConstStatement(Token constToken, Token nameToken, string ns, TopLevelConstruct owner, Library library)
            : base(constToken, owner)
        {
            this.Library = library;
            this.NameToken = nameToken;
            this.Name = nameToken.Value;
            this.Namespace = ns;
        }

        internal override IList<Executable> Resolve(Parser parser)
        {
            ConstantResolutionState resolutionState = parser.ConstantAndEnumResolutionState[this];
            if (resolutionState == ConstantResolutionState.RESOLVED) return new Executable[0];
            if (resolutionState == ConstantResolutionState.RESOLVING)
            {
                throw new ParserException(this.FirstToken, "The resolution of this enum creates a cycle.");
            }
            parser.ConstantAndEnumResolutionState[this] = ConstantResolutionState.RESOLVING;

            this.Expression = this.Expression.Resolve(parser);

            if (this.Expression is IntegerConstant ||
                this.Expression is BooleanConstant ||
                this.Expression is FloatConstant ||
                this.Expression is StringConstant)
            {
                // that's fine.
            }
            else
            {
                throw new ParserException(this.FirstToken, "Invalid value for const. Expression must resolve to a constant at compile time.");
            }

            parser.ConstantAndEnumResolutionState[this] = ConstantResolutionState.RESOLVED;

            return new Executable[0];
        }

        internal override Executable ResolveNames(Parser parser, Dictionary<string, TopLevelConstruct> lookup, string[] imports)
        {
            this.Expression = this.Expression.ResolveNames(parser, lookup, imports);
            return this;
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }
        internal override void PerformLocalIdAllocation(Parser parser, VariableIdAllocator varIds, VariableIdAllocPhase phase) { }
    }
}
