using Parser.Resolver;
using System;
using System.Collections.Generic;

namespace Parser.ParseTree
{
    internal class ListSlice : Expression
    {
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

        internal override IEnumerable<Expression> Descendants
        {
            get
            {
                List<Expression> output = new List<Expression>() { this.Root };
                for (int i = 0; i < 3; ++i)
                {
                    if (this.Items[i] != null)
                    {
                        output.Add(this.Items[i]);
                    }
                }
                return output;
            }
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

        internal override Expression ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            this.Root = this.Root.ResolveTypes(parser, typeResolver);
            for (int i = 0; i < this.Items.Length; ++i)
            {
                if (this.Items[i] != null)
                {
                    this.Items[i] = this.Items[i].ResolveTypes(parser, typeResolver);
                    if (!this.Items[i].ResolvedType.CanAssignToA(ResolvedType.INTEGER))
                    {
                        throw new ParserException(this.Items[i], "List/string slice arguments must be integers.");
                    }
                }
            }

            if (this.Root.ResolvedType == ResolvedType.ANY)
            {
                this.ResolvedType = ResolvedType.ANY;
            }
            else if (this.Root.ResolvedType == ResolvedType.STRING)
            {
                this.ResolvedType = ResolvedType.STRING;
            }
            else if (this.Root.ResolvedType.Category == ResolvedTypeCategory.LIST)
            {
                this.ResolvedType = this.Root.ResolvedType;
            }
            else
            {
                throw new ParserException(this.BracketToken, "Cannot perform slicing on this type.");
            }
            return this;
        }

        internal override void ResolveVariableOrigins(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase)
        {
            this.Root.ResolveVariableOrigins(parser, varIds, phase);
            foreach (Expression item in this.Items)
            {
                if (item != null)
                {
                    item.ResolveVariableOrigins(parser, varIds, phase);
                }
            }
        }
    }
}
