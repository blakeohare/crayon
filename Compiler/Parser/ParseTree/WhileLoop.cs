using Parser.Resolver;
using System.Collections.Generic;
using System.Linq;

namespace Parser.ParseTree
{
    public class WhileLoop : Executable
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

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase)
        {
            this.Condition.PerformLocalIdAllocation(parser, varIds, phase);

            if (phase != VariableIdAllocPhase.REGISTER_AND_ALLOC)
            {
                foreach (Executable ex in this.Code)
                {
                    ex.PerformLocalIdAllocation(parser, varIds, phase);
                }
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
            }
        }

        internal override void ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            throw new System.NotImplementedException();
        }
    }
}
