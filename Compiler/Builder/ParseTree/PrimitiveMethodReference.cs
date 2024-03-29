﻿using Builder.Resolver;
using System;
using System.Collections.Generic;

namespace Builder.ParseTree
{
    internal class PrimitiveMethodReference : Expression
    {
        public Expression Root { get; private set; }
        public Token DotToken { get; private set; }
        public Token FieldToken { get; private set; }
        public string Field { get; private set; }

        public PrimitiveMethodReference(Expression root, Token dotToken, Token fieldToken, ResolvedType resolvedType, Node owner)
            : base(root.FirstToken, owner)
        {
            this.Root = root;
            this.DotToken = dotToken;
            this.FieldToken = fieldToken;
            this.Field = fieldToken.Value;
            this.ResolvedType = resolvedType;
        }

        internal override IEnumerable<Expression> Descendants { get { return new Expression[] { this.Root }; } }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            // Generated in the ResolveEntityNames phase, so this shouldn't get called.
            throw new Exception();
        }

        internal override void ResolveVariableOrigins(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase)
        {
            throw new NotImplementedException();
        }

        internal override Expression ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            return this;
        }

        internal override Expression Resolve(ParserContext parser)
        {
            this.Root = this.Root.Resolve(parser);
            return this;
        }
    }
}
