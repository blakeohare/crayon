using Parser.Resolver;
using System.Collections.Generic;

namespace Parser.ParseTree
{
    public class CompileTimeDictionary : Expression
    {
        public override bool CanAssignTo { get { return false; } }

        internal override IEnumerable<Expression> Descendants { get { return Expression.NO_DESCENDANTS; } }

        public string Type { get; private set; }

        public CompileTimeDictionary(Token firstToken, string type, Node owner)
            : base(firstToken, owner)
        {
            this.Type = type;
        }

        internal override Expression Resolve(ParserContext parser)
        {
            return this;
        }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            return this;
        }

        internal override Expression ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            throw new System.NotImplementedException();
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase) { }
    }
}
