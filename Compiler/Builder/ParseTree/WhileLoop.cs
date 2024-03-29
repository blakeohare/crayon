﻿using Builder.Resolver;
using System.Collections.Generic;
using System.Linq;

namespace Builder.ParseTree
{
    internal class WhileLoop : Executable
    {
        public Expression Condition { get; private set; }
        public Executable[] Code { get; private set; }

        public WhileLoop(Token whileToken, Expression condition, IList<Executable> code, Node owner)
            : base(whileToken, owner)
        {
            this.Condition = condition;
            this.Code = code.ToArray();
        }

        internal override IList<Executable> Resolve(ParserContext parser)
        {
            this.Condition = this.Condition.Resolve(parser);
            this.Code = Resolve(parser, this.Code).ToArray();
            return Listify(this);
        }

        internal override Executable ResolveEntityNames(ParserContext parser)
        {
            this.Condition = this.Condition.ResolveEntityNames(parser);
            this.BatchExecutableEntityNameResolver(parser, this.Code);
            return this;
        }

        internal override void ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            this.Condition = this.Condition.ResolveTypes(parser, typeResolver);
            if (!this.Condition.ResolvedType.CanAssignToA(parser.TypeContext.BOOLEAN))
            {
                throw new ParserException(this.Condition, "Can only use a boolean for while loop conditions.");
            }

            foreach (Executable ex in this.Code)
            {
                ex.ResolveTypes(parser, typeResolver);
            }
        }

        internal override void ResolveVariableOrigins(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase)
        {
            this.Condition.ResolveVariableOrigins(parser, varIds, phase);

            if (phase != VariableIdAllocPhase.REGISTER_AND_ALLOC)
            {
                foreach (Executable ex in this.Code)
                {
                    ex.ResolveVariableOrigins(parser, varIds, phase);
                }
            }
            else
            {
                foreach (Executable ex in this.Code)
                {
                    ex.ResolveVariableOrigins(parser, varIds, VariableIdAllocPhase.REGISTER);
                }

                foreach (Executable ex in this.Code)
                {
                    ex.ResolveVariableOrigins(parser, varIds, VariableIdAllocPhase.ALLOC);
                }
            }
        }
    }
}
