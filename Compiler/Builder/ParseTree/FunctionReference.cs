﻿using Builder.Resolver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Builder.ParseTree
{
    internal class FunctionReference : Expression
    {
        internal override IEnumerable<Expression> Descendants { get { return Expression.NO_DESCENDANTS; } }

        public FunctionDefinition FunctionDefinition { get; set; }

        public FunctionReference(Token token, FunctionDefinition funcDef, Node owner)
            : base(token, owner)
        {
            this.FunctionDefinition = funcDef;
        }

        internal override Expression Resolve(ParserContext parser)
        {
            return this;
        }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            throw new InvalidOperationException(); // Generated in the resolve name phase.
        }

        internal override Expression ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            ResolvedType returnType = this.FunctionDefinition.ResolvedReturnType;
            ResolvedType[] argTypes = this.FunctionDefinition.ResolvedArgTypes;
            int optionalCount = argTypes.Length == 0 || this.FunctionDefinition.DefaultValues[argTypes.Length - 1] == null
                ? 0
                : this.FunctionDefinition.DefaultValues.Where(e => e != null).Count();
            this.ResolvedType = ResolvedType.GetFunctionType(returnType, argTypes, optionalCount);
            return this;
        }

        internal override void ResolveVariableOrigins(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase) { }
    }
}
