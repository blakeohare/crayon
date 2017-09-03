using System;
using System.Collections.Generic;

namespace Crayon.ParseTree
{
    internal class ListSlice : Expression
    {
        internal override Expression PastelResolve(Parser parser)
        {
            throw new NotImplementedException();
        }

        public override bool CanAssignTo { get { return false; } }

        public Token BracketToken { get; set; }
        public Expression[] Items { get; set; } // these can be null
        public Expression Root { get; set; }

        public ListSlice(Expression root, List<Expression> items, Token bracketToken, Executable owner)
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

        internal override Expression Resolve(Parser parser)
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

        internal override Expression ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
        {
            this.Root = this.Root.ResolveNames(parser, lookup, imports);
            this.BatchExpressionNameResolver(parser, lookup, imports, this.Items);
            return this;
        }

        internal override void PerformLocalIdAllocation(Parser parser, VariableIdAllocator varIds, VariableIdAllocPhase phase)
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

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars)
        {
            throw new System.NotImplementedException();
        }
    }
}
