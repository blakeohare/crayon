using System;
using System.Collections.Generic;

namespace Crayon.Pastel.Nodes
{
    class InlineConstant : Expression
    {
        public object Value { get; set; }
        public PType Type { get; set; }

        public InlineConstant(PType type, Token firstToken, object value) : base(firstToken)
        {
            this.Type = type;
            this.Value = value;
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
