using Parser.Resolver;
using System.Collections.Generic;
using System.Linq;

namespace Parser.ParseTree
{
    public class DoWhileLoop : Executable
    {
        public Executable[] Code { get; private set; }
        public Expression Condition { get; private set; }

        public DoWhileLoop(Token doToken, IList<Executable> code, Expression condition, Node owner)
            : base(doToken, owner)
        {
            this.Code = code.ToArray();
            this.Condition = condition;
        }

        internal override IList<Executable> Resolve(ParserContext parser)
        {
            this.Code = Resolve(parser, this.Code).ToArray();

            this.Condition = this.Condition.Resolve(parser);

            return Listify(this);
        }

        internal override Executable ResolveEntityNames(ParserContext parser)
        {
            this.BatchExecutableEntityNameResolver(parser, this.Code);
            this.Condition = this.Condition.ResolveEntityNames(parser);
            return this;
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase)
        {
            if (phase != VariableIdAllocPhase.REGISTER_AND_ALLOC)
            {
                foreach (Executable ex in this.Code)
                {
                    ex.PerformLocalIdAllocation(parser, varIds, phase);
                }
                this.Condition.PerformLocalIdAllocation(parser, varIds, phase);
            }
            else
            {
                foreach (Executable ex in this.Code)
                {
                    ex.PerformLocalIdAllocation(parser, varIds, VariableIdAllocPhase.REGISTER);
                }
                foreach (Executable ex in this.Code)
                {
                    ex.PerformLocalIdAllocation(parser, varIds, VariableIdAllocPhase.ALLOC);
                }
                this.Condition.PerformLocalIdAllocation(parser, varIds, VariableIdAllocPhase.ALLOC);
            }
        }

        internal override void ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            throw new System.NotImplementedException();
        }
    }
}
