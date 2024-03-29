﻿using Builder.Resolver;
using System;
using System.Collections.Generic;

namespace Builder.ParseTree
{
    internal class ClassReference : Expression
    {
        internal override IEnumerable<Expression> Descendants { get { return Expression.NO_DESCENDANTS; } }

        public ClassDefinition ClassDefinition { get; private set; }

        public ClassReference(Token token, ClassDefinition clazz, Node owner)
            : base(token, owner)
        {
            this.ClassDefinition = clazz;
        }

        internal override Expression Resolve(ParserContext parser)
        {
            // normal usages should be optimized out by now.
            throw new ParserException(this, "Unexpected class reference.");
        }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            throw new InvalidOperationException(); // Created during the resolve names phase.
        }

        internal override Expression ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            throw new ParserException(this.FirstToken, "The class '" + this.ClassDefinition.NameToken.Value + "' cannot be used here.");
        }

        internal override void ResolveVariableOrigins(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase) { }
    }
}
