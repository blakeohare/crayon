using System.Collections.Generic;

namespace Crayon.ParseTree
{
    internal class ThrowStatement : Executable
    {
        public override bool IsTerminator { get { return true; } }

        public Expression Expression { get; set; }
        public Token ThrowToken { get; set; }

        public ThrowStatement(Token throwToken, Expression expression, TopLevelConstruct owner) : base(throwToken, owner)
        {
            this.ThrowToken = throwToken;
            this.Expression = expression;
        }

        internal override IList<Executable> Resolve(Parser parser)
        {
            this.Expression = this.Expression.Resolve(parser);
            return Listify(this);
        }

        internal override Executable ResolveNames(Parser parser, Dictionary<string, TopLevelConstruct> lookup, string[] imports)
        {
            this.Expression.ResolveNames(parser, lookup, imports);
            return this;
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars)
        {
            this.Expression.GetAllVariablesReferenced(vars);
        }

        internal override void PerformLocalIdAllocation(Parser parser, VariableIdAllocator varIds, VariableIdAllocPhase phase)
        {
            this.Expression.PerformLocalIdAllocation(parser, varIds, phase);
        }

        internal override Executable PastelResolve(Parser parser)
        {
            throw new System.NotImplementedException();
        }
    }
}
