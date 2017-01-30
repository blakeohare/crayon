using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pastel.Nodes
{
    class InlineIncrement : Expression
    {
        public Token IncrementToken { get; set; }
        public Expression Expression { get; set; }
        public bool IsPrefix { get; set; }

        public InlineIncrement(Token firstToken, Token incrementToken, Expression root, bool isPrefix) : base(firstToken)
        {
            this.IncrementToken = incrementToken;
            this.Expression = root;
            this.IsPrefix = isPrefix;
        }

        public override Expression NameResolution(Dictionary<string, FunctionDefinition> functionLookup, Dictionary<string, StructDefinition> structLookup)
        {
            throw new NotImplementedException();
        }

        public override void ResolveTypes()
        {
            throw new NotImplementedException();
        }
    }
}
