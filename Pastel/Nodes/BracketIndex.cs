using System;
using System.Collections.Generic;

namespace Pastel.Nodes
{
    class BracketIndex : Expression
    {
        public Expression Root { get; set; }
        public Token BracketToken { get; set; }
        public Expression Index { get; set; }

        public BracketIndex(Expression root, Token bracketToken, Expression index) : base(root.FirstToken)
        {
            this.Root = root;
            this.BracketToken = bracketToken;
            this.Index = index;
        }

        public override Expression NameResolution(Dictionary<string, FunctionDefinition> functionLookup, Dictionary<string, StructDefinition> structLookup)
        {
            throw new NotImplementedException();
        }

        public override void ResolveTypes()
        {
            this.Root.ResolveTypes();
            this.Index.ResolveTypes();
            throw new NotImplementedException();
        }
    }
}
