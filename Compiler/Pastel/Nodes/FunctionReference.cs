﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pastel.Nodes
{
    public class FunctionReference : Expression
    {
        public FunctionDefinition Function { get; set; }
        public bool IsLibraryScopedFunction { get; set; }

        public FunctionReference(Token firstToken, FunctionDefinition functionDefinition) : base(firstToken)
        {
            this.Function = functionDefinition;
            this.IsLibraryScopedFunction = false;
        }

        public override Expression ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            return this;
        }

        internal override Expression ResolveType(VariableScope varScope, PastelCompiler compiler)
        {
            // no function pointeer type.
            return this;
        }

        internal override Expression ResolveWithTypeContext(PastelCompiler compiler)
        {
            return this;
        }
    }
}
