﻿using System;
using System.Collections.Generic;

namespace Parser.ParseTree
{
    public class FunctionReference : Expression
    {
        public override bool CanAssignTo { get { return false; } }

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

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase) { }
    }
}
