using System;
using System.Collections.Generic;

namespace Parser.ParseTree
{
    public class ListSlice : Expression
    {
        public override bool CanAssignTo { get { return false; } }

        public Token BracketToken { get; set; }
        public Expression[] Items { get; set; } // these can be null
        public Expression Root { get; set; }

        public ListSlice(Expression root, List<Expression> items, Token bracketToken, Node owner)
            : base(root.FirstToken, owner)
        {
            this.Root = root;
            this.BracketToken = bracketToken;
            if (items.Count == 2)
            {
                items.Add(new IntegerConstant(null, 1, owner));
            }

            if (items.Count != 3)
            {
                throw new Exception("Slices must have 2 or 3 components before passed into the constructor.");
            }

            if (items[2] == null)
            {
                items[2] = new IntegerConstant(null, 1, owner);
            }

            this.Items = items.ToArray();
        }

        internal override Expression Resolve(ParserContext parser)
        {
            this.Root = this.Root.Resolve(parser);
            for (int i = 0; i < this.Items.Length; ++i)
            {
                Expression item = this.Items[i];
                if (item != null)
                {
                    this.Items[i] = this.Items[i].Resolve(parser);
                }
            }
            return this;
        }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            this.Root = this.Root.ResolveEntityNames(parser);
            this.BatchExpressionEntityNameResolver(parser, this.Items);
            return this;
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase)
        {
            this.Root.PerformLocalIdAllocation(parser, varIds, phase);
            foreach (Expression item in this.Items)
            {
                if (item != null)
                {
                    item.PerformLocalIdAllocation(parser, varIds, phase);
                }
            }
        }
    }
}
