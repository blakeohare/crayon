using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pastel.Nodes
{
    class CompileTimeFunctionReference : Expression
    {
        public Token NameToken { get; set; }

        public CompileTimeFunctionReference(Token atToken, Token nameToken) : base(atToken)
        {
            this.NameToken = nameToken;
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
