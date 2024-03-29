﻿using Builder.Resolver;
using System.Collections.Generic;
using System.Linq;

namespace Builder.ParseTree
{
    internal class IfStatement : Executable
    {
        public Expression Condition { get; private set; }
        public Executable[] TrueCode { get; private set; }
        public Executable[] FalseCode { get; private set; }

        public IfStatement(
            Token ifToken,
            Expression condition,
            IList<Executable> trueCode,
            IList<Executable> falseCode,
            Node owner)
            : base(ifToken, owner)
        {
            this.Condition = condition;
            this.TrueCode = trueCode.ToArray();
            this.FalseCode = falseCode.ToArray();
        }

        internal override IList<Executable> Resolve(ParserContext parser)
        {
            this.Condition = this.Condition.Resolve(parser);

            this.TrueCode = Resolve(parser, this.TrueCode).ToArray();
            this.FalseCode = Resolve(parser, this.FalseCode).ToArray();

            BooleanConstant bc = this.Condition as BooleanConstant;
            if (bc != null)
            {
                return bc.Value ? this.TrueCode : this.FalseCode;
            }

            return Listify(this);
        }

        public override bool IsTerminator
        {
            get
            {
                return this.TrueCode.Length > 0 &&
                    this.FalseCode.Length > 0 &&
                    this.TrueCode[this.TrueCode.Length - 1].IsTerminator &&
                    this.FalseCode[this.FalseCode.Length - 1].IsTerminator;
            }
        }

        internal override Executable ResolveEntityNames(ParserContext parser)
        {
            this.Condition = this.Condition.ResolveEntityNames(parser);
            this.BatchExecutableEntityNameResolver(parser, this.TrueCode);
            this.BatchExecutableEntityNameResolver(parser, this.FalseCode);
            return this;
        }

        internal override void ResolveVariableOrigins(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase)
        {
            this.Condition.ResolveVariableOrigins(parser, varIds, phase);
            if (phase != VariableIdAllocPhase.REGISTER_AND_ALLOC || this.TrueCode.Length == 0 || this.FalseCode.Length == 0)
            {
                foreach (Executable ex in this.TrueCode.Concat(this.FalseCode))
                {
                    ex.ResolveVariableOrigins(parser, varIds, phase);
                }
            }
            else
            {
                // branch the variable ID allocator.
                VariableScope trueVars = VariableScope.CreatedNestedBlockScope(varIds);
                VariableScope falseVars = VariableScope.CreatedNestedBlockScope(varIds);

                // Go through and register and allocate all variables.
                // The allocated ID's are going to be garbage, but this enforces that they must be
                // declared before being used.
                foreach (Executable ex in this.TrueCode)
                {
                    ex.ResolveVariableOrigins(parser, trueVars, VariableIdAllocPhase.REGISTER_AND_ALLOC);
                }
                foreach (Executable ex in this.FalseCode)
                {
                    ex.ResolveVariableOrigins(parser, falseVars, VariableIdAllocPhase.REGISTER_AND_ALLOC);
                }

                // Now that the code is as correct as we can verify, merge the branches back together
                // creating a new set of variable ID's.
                trueVars.MergeToParent();
                falseVars.MergeToParent();

                if (!this.CompilationScope.IsStaticallyTyped)
                {
                    // Go back through and do another allocation pass and assign the correct variable ID's.
                    foreach (Executable ex in this.TrueCode.Concat(this.FalseCode))
                    {
                        ex.ResolveVariableOrigins(parser, varIds, VariableIdAllocPhase.ALLOC);
                    }
                }
            }
        }

        internal override void ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            this.Condition = this.Condition.ResolveTypes(parser, typeResolver);
            ResolvedTypeCategory type = this.Condition.ResolvedType.Category;
            if (type != ResolvedTypeCategory.BOOLEAN && type != ResolvedTypeCategory.ANY)
            {
                throw new ParserException(this.Condition, "Boolean expected.");
            }

            foreach (Executable ex in this.TrueCode.Concat(this.FalseCode))
            {
                ex.ResolveTypes(parser, typeResolver);
            }
        }
    }
}
