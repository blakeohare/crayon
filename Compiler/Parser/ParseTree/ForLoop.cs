using Parser.Resolver;
using System.Collections.Generic;
using System.Linq;

namespace Parser.ParseTree
{
    public class ForLoop : Executable
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

        internal override void ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            throw new System.NotImplementedException();
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase)
        {
            bool register = (phase & VariableIdAllocPhase.REGISTER) != 0;
            bool alloc = (phase & VariableIdAllocPhase.ALLOC) != 0;
            bool both = register && alloc;

            foreach (Executable ex in this.Init)
            {
                ex.PerformLocalIdAllocation(parser, varIds, phase);
            }

            this.Condition.PerformLocalIdAllocation(parser, varIds, phase);

            if (both)
            {
                foreach (Executable ex in this.Code.Concat(this.Step))
                {
                    ex.PerformLocalIdAllocation(parser, varIds, VariableIdAllocPhase.REGISTER);
                }

                foreach (Executable ex in this.Code.Concat(this.Step))
                {
                    ex.PerformLocalIdAllocation(parser, varIds, VariableIdAllocPhase.ALLOC);
                }
            }
            else
            {
                foreach (Executable ex in this.Code.Concat(this.Step))
                {
                    ex.PerformLocalIdAllocation(parser, varIds, phase);
                }
            }
        }

        internal override void ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            throw new System.NotImplementedException();
        }
    }
}
