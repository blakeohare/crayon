using System;
using System.Collections.Generic;

namespace Pastel.Nodes
{
    class DotField : Expression
    {
        public Expression Root { get; set; }
        public Token DotToken { get; set; }
        public Token FieldName { get; set; }

        public DotField(Expression root, Token dotToken, Token fieldName) : base(root.FirstToken)
        {
            this.Root = root;
            this.DotToken = dotToken;
            this.FieldName = fieldName;
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
