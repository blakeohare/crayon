using System.Collections.Generic;

namespace Crayon.ParseTree
{
    internal class ConstStatement : TopLevelConstruct
    {
        public Expression Expression { get; set; }
        public Token NameToken { get; private set; }
        public string Name { get; private set; }

        public ConstStatement(Token constToken, Token nameToken, string ns, TopLevelConstruct owner, Library library, FileScope fileScope)
            : base(constToken, owner, fileScope)
        {
            this.Library = library;
            this.NameToken = nameToken;
            this.Name = nameToken.Value;
            this.Namespace = ns;
        }

        internal override void Resolve(ParserContext parser)
        {
            ConstantResolutionState resolutionState = parser.ConstantAndEnumResolutionState[this];
            if (resolutionState == ConstantResolutionState.RESOLVED) return;
            if (resolutionState == ConstantResolutionState.RESOLVING)
            {
                throw new ParserException(this.FirstToken, "The resolution of this enum creates a cycle.");
            }
            parser.ConstantAndEnumResolutionState[this] = ConstantResolutionState.RESOLVING;

            this.Expression = this.Expression.Resolve(parser);

            if (!(this.Expression is IConstantValue))
            {
                throw new ParserException(this.FirstToken, "Invalid value for const. Expression must resolve to a constant at compile time.");
            }

            parser.ConstantAndEnumResolutionState[this] = ConstantResolutionState.RESOLVED;
        }

        internal override void ResolveNames(ParserContext parser)
        {
            this.Expression = this.Expression.ResolveNames(parser);
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }
        internal override void PerformLocalIdAllocation(ParserContext parser, VariableIdAllocator varIds, VariableIdAllocPhase phase) { }
    }
}
