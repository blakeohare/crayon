using System.Collections.Generic;
using System.Linq;

namespace Parser.ParseTree
{
    public class DoWhileLoop : Executable
    {
        public Executable[] Code { get; private set; }
        public Expression Condition { get; private set; }

        public DoWhileLoop(Token doToken, IList<Executable> code, Expression condition, TopLevelConstruct owner)
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

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableIdAllocator varIds, VariableIdAllocPhase phase)
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

        internal override Executable ResolveNames(ParserContext parser)
        {
            this.BatchExecutableNameResolver(parser, this.Code);
            this.Condition = this.Condition.ResolveNames(parser);
            return this;
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars)
        {
            this.Condition.GetAllVariablesReferenced(vars);
            for (int i = 0; i < this.Code.Length; ++i)
            {
                this.Code[i].GetAllVariablesReferenced(vars);
            }
        }

        internal override Executable PastelResolve(ParserContext parser)
        {
            throw new System.NotImplementedException();
        }
    }
}
