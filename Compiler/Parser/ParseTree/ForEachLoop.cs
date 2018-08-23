using System.Collections.Generic;
using System.Linq;

namespace Parser.ParseTree
{
    public class ForEachLoop : Executable
    {
        public Token IterationVariable { get; private set; }
        public int IterationVariableId { get; private set; }
        public Expression IterationExpression { get; private set; }
        public Executable[] Code { get; private set; }

        public ForEachLoop(Token forToken, Token iterationVariable, Expression iterationExpression, IList<Executable> code, TopLevelConstruct owner)
            : base(forToken, owner)
        {
            this.IterationVariable = iterationVariable;
            this.IterationExpression = iterationExpression;
            this.Code = code.ToArray();
        }

        internal override IList<Executable> Resolve(ParserContext parser)
        {
            this.IterationExpression = this.IterationExpression.Resolve(parser);
            parser.ValueStackDepth += 3;
            this.Code = Resolve(parser, this.Code).ToArray();
            parser.ValueStackDepth -= 3;
            return Listify(this);
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableIdAllocator varIds, VariableIdAllocPhase phase)
        {
            this.IterationExpression.PerformLocalIdAllocation(parser, varIds, phase);

            if ((phase & VariableIdAllocPhase.REGISTER) != 0)
            {
                varIds.RegisterVariable(this.IterationVariable.Value);
            }

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

            this.IterationVariableId = varIds.GetVarId(this.IterationVariable);
        }

        internal override Executable ResolveEntityNames(ParserContext parser)
        {
            this.IterationExpression = this.IterationExpression.ResolveEntityNames(parser);
            this.BatchExecutableEntityNameResolver(parser, this.Code);
            return this;
        }
    }
}
