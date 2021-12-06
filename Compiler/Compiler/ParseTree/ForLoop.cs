using Builder.Resolver;
using System.Collections.Generic;
using System.Linq;

namespace Builder.ParseTree
{
    internal class ForLoop : Executable
    {
        public Executable[] Init { get; private set; }
        public Expression Condition { get; private set; }
        public Executable[] Step { get; private set; }
        public Executable[] Code { get; private set; }

        public ForLoop(
            Token forToken,
            IList<Executable> init,
            Expression condition,
            IList<Executable> step,
            IList<Executable> code,
            Node owner)
            : base(forToken, owner)
        {
            this.Init = init.ToArray();
            this.Condition = condition ?? new BooleanConstant(forToken, true, owner);
            this.Step = step.ToArray();
            this.Code = code.ToArray();
        }

        internal override IList<Executable> Resolve(ParserContext parser)
        {
            this.Init = Resolve(parser, this.Init).ToArray();
            this.Condition = this.Condition.Resolve(parser);
            this.Step = Resolve(parser, this.Step).ToArray();
            this.Code = Resolve(parser, this.Code).ToArray();

            return Listify(this);
        }

        internal override Executable ResolveEntityNames(ParserContext parser)
        {
            this.BatchExecutableEntityNameResolver(parser, this.Init);
            this.Condition = this.Condition.ResolveEntityNames(parser);
            this.BatchExecutableEntityNameResolver(parser, this.Step);
            this.BatchExecutableEntityNameResolver(parser, this.Code);
            return this;
        }

        internal override void ResolveVariableOrigins(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase)
        {
            bool register = (phase & VariableIdAllocPhase.REGISTER) != 0;
            bool alloc = (phase & VariableIdAllocPhase.ALLOC) != 0;
            bool both = register && alloc;

            foreach (Executable ex in this.Init)
            {
                ex.ResolveVariableOrigins(parser, varIds, phase);
            }

            this.Condition.ResolveVariableOrigins(parser, varIds, phase);

            if (both)
            {
                foreach (Executable ex in this.Code.Concat(this.Step))
                {
                    ex.ResolveVariableOrigins(parser, varIds, VariableIdAllocPhase.REGISTER);
                }

                foreach (Executable ex in this.Code.Concat(this.Step))
                {
                    ex.ResolveVariableOrigins(parser, varIds, VariableIdAllocPhase.ALLOC);
                }
            }
            else
            {
                foreach (Executable ex in this.Code.Concat(this.Step))
                {
                    ex.ResolveVariableOrigins(parser, varIds, phase);
                }
            }
        }

        internal override void ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            foreach (Executable init in this.Init)
            {
                init.ResolveTypes(parser, typeResolver);
            }

            this.Condition = this.Condition.ResolveTypes(parser, typeResolver);
            if (!this.Condition.ResolvedType.CanAssignToA(parser.TypeContext.BOOLEAN))
            {
                throw new ParserException(this.Condition, "for loop condition must be a boolean.");
            }

            foreach (Executable step in this.Step)
            {
                step.ResolveTypes(parser, typeResolver);
            }
            foreach (Executable line in this.Code)
            {
                line.ResolveTypes(parser, typeResolver);
            }
        }
    }
}
