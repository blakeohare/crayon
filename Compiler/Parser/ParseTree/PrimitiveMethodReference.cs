using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parser.Resolver;

namespace Parser.ParseTree
{
    internal class PrimitiveMethodReference : Expression
    {
        public override bool CanAssignTo { get { return false; } }
        public Expression Root { get; private set; }
        public Token FieldToken { get; private set; }
        public string Field { get; private set; }
        public PrimitiveMethodReference(Expression root, Token fieldToken, ResolvedType resolvedType, Node owner)
            : base(root.FirstToken, owner)
        {
            this.Root = root;
            this.FieldToken = fieldToken;
            this.Field = fieldToken.Value;
            this.ResolvedType = resolvedType;
        }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            // Generated in the ResolveEntityNames phase, so this shouldn't get called.
            throw new Exception();
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase)
        {
            throw new NotImplementedException();
        }

        internal override Expression ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            throw new NotImplementedException();
        }

        internal override Expression Resolve(ParserContext parser)
        {
            throw new NotImplementedException();
        }

    }
}
