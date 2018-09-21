using Parser.Resolver;
using System.Collections.Generic;
using System.Linq;

namespace Parser.ParseTree
{
    public class ListDefinition : Expression
    {
        public Expression[] Items { get; private set; }
        public AType ListType { get; private set; }

        public ListDefinition(Token openBracket, IList<Expression> items, AType listType, Node owner)
            : base(openBracket, owner)
        {
            this.Items = items.ToArray();
            this.ListType = listType;
        }

        internal override IEnumerable<Expression> Descendants { get { return this.Items; } }

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

        internal override Expression ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            ResolvedType itemType = typeResolver.ResolveType(this.ListType);
            this.ResolvedType = ResolvedType.ListOrArrayOf(itemType);
            for (int i = 0; i < this.Items.Length; ++i)
            {
                this.Items[i] = this.Items[i].ResolveTypes(parser, typeResolver);
                if (!this.Items[i].ResolvedType.CanAssignToA(itemType))
                {
                    throw new ParserException(this.Items[i], "This item cannot exist in a list of this type.");
                }
            }

            return this;
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase)
        {
            foreach (Expression item in this.Items)
            {
                item.PerformLocalIdAllocation(parser, varIds, phase);
            }
        }
    }
}
