﻿using System;
using System.Collections.Generic;

namespace Crayon.ParseTree
{
    internal class FieldReference : Expression
    {
        internal override Expression PastelResolve(Parser parser)
        {
            throw new NotImplementedException();
        }

        public override bool IsInlineCandidate
        {
            get
            {
                return this.Field.IsStaticField;
            }

        }

        public override bool CanAssignTo { get { return true; } }

        public FieldDeclaration Field { get; set; }

        public FieldReference(Token token, FieldDeclaration field, TopLevelConstruct owner)
            : base(token, owner)
        {
            this.Field = field;
        }

        internal override Expression Resolve(Parser parser)
        {
            return this;
        }

        internal override Expression ResolveNames(Parser parser)
        {
            throw new InvalidOperationException(); // created in the resolve name phase.
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }

        internal override void PerformLocalIdAllocation(Parser parser, VariableIdAllocator varIds, VariableIdAllocPhase phase) { }
    }
}
