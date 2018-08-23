using System;
using System.Collections.Generic;
using System.Linq;

namespace Parser.ParseTree
{
    public class ListDefinition : Expression
    {
        public override bool CanAssignTo { get { return false; } }

        public Expression[] Items { get; private set; }
        public ListDefinition(Token openBracket, IList<Expression> items, TopLevelConstruct owner)
            : base(openBracket, owner)
        {
            this.Items = items.ToArray();
        }

        internal override Expression Resolve(ParserContext parser)
        {
            for (int i = 0; i < this.Items.Length; ++i)
            {
                this.Items[i] = this.Items[i].Resolve(parser);
            }

            return this;
        }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            this.BatchExpressionEntityNameResolver(parser, this.Items);
            return this;
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableIdAllocator varIds, VariableIdAllocPhase phase)
        {
            foreach (Expression item in this.Items)
            {
                item.PerformLocalIdAllocation(parser, varIds, phase);
            }
        }
    }
}
