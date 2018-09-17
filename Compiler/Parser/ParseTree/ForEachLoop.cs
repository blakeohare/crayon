using Parser.Resolver;
using System.Collections.Generic;
using System.Linq;

namespace Parser.ParseTree
{
    public class ForEachLoop : Executable
    {
        public Token IterationVariable { get; private set; }
        public VariableId IterationVariableId { get; private set; }
        public VariableId IndexLocalId { get; private set; }
        public VariableId ListLocalId { get; private set; }
        public Expression IterationExpression { get; private set; }
        public Executable[] Code { get; private set; }
        public AType IterationType { get; private set; }

        public ForEachLoop(Token forToken, AType iterationType, Token iterationVariable, Expression iterationExpression, IList<Executable> code, Node owner)
            : base(forToken, owner)
        {
            this.IterationVariable = iterationVariable;
            this.IterationType = iterationType;
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

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase)
        {
            this.IterationExpression.PerformLocalIdAllocation(parser, varIds, phase);

            if ((phase & VariableIdAllocPhase.REGISTER) != 0)
            {
                varIds.RegisterVariable(this.IterationType, this.IterationVariable.Value);
                this.IndexLocalId = varIds.RegisterSyntheticVariable(AType.Integer(this.FirstToken));
                this.ListLocalId = varIds.RegisterSyntheticVariable(AType.Any(this.FirstToken));
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

        internal override void ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            throw new System.NotImplementedException();
        }
    }
}
