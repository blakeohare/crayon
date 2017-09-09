using System;
using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
    internal class ForEachLoop : Executable
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

        internal override IList<Executable> Resolve(Parser parser)
        {
            this.IterationExpression = this.IterationExpression.Resolve(parser);
            parser.ValueStackDepth += 3;
            this.Code = Resolve(parser, this.Code).ToArray();
            parser.ValueStackDepth -= 3;
            return Listify(this);
        }

        internal override void PerformLocalIdAllocation(Parser parser, VariableIdAllocator varIds, VariableIdAllocPhase phase)
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

        internal override Executable ResolveNames(Parser parser, Dictionary<string, TopLevelConstruct> lookup, string[] imports)
        {
            this.IterationExpression = this.IterationExpression.ResolveNames(parser, lookup, imports);
            this.BatchExecutableNameResolver(parser, lookup, imports, this.Code);
            return this;
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars)
        {
            this.IterationExpression.GetAllVariablesReferenced(vars);
            for (int i = 0; i < this.Code.Length; ++i)
            {
                this.Code[i].GetAllVariablesReferenced(vars);
            }
        }

        internal override Executable PastelResolve(Parser parser)
        {
            throw new System.NotImplementedException();
        }
    }
}
