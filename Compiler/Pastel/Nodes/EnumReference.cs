﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pastel.Nodes
{
    class EnumReference : Expression
    {
        public EnumDefinition EnumDef { get; set; }

        public EnumReference(Token firstToken, EnumDefinition enumDef) : base(firstToken)
        {
            this.EnumDef = enumDef;
        }

        public override Expression ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            // created by this phase
            throw new NotImplementedException();
        }

        internal override Expression ResolveType(VariableScope varScope, PastelCompiler compiler)
        {
            // should be resolved out by now
            throw new NotImplementedException();
        }

        internal override Expression ResolveWithTypeContext(PastelCompiler compiler)
        {
            throw new NotImplementedException();
        }
    }
}
