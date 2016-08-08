using System;
using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
    internal class IfStatement : Executable
    {
        public Expression Condition { get; private set; }
        public Executable[] TrueCode { get; private set; }
        public Executable[] FalseCode { get; private set; }

        public IfStatement(Token ifToken, Expression condition, IList<Executable> trueCode, IList<Executable> falseCode, Executable owner)
            : base(ifToken, owner)
        {
            this.Condition = condition;
            this.TrueCode = trueCode.ToArray();
            this.FalseCode = falseCode.ToArray();
        }

        internal override IList<Executable> Resolve(Parser parser)
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

        internal override void GetAllVariableNames(Dictionary<string, bool> lookup)
        {
            foreach (Executable line in this.TrueCode)
            {
                line.GetAllVariableNames(lookup);
            }

            foreach (Executable line in this.FalseCode)
            {
                line.GetAllVariableNames(lookup);
            }
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
        

        internal override Executable ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
        {
            this.Condition = this.Condition.ResolveNames(parser, lookup, imports);
            this.BatchExecutableNameResolver(parser, lookup, imports, this.TrueCode);
            this.BatchExecutableNameResolver(parser, lookup, imports, this.FalseCode);
            return this;
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars)
        {
            this.Condition.GetAllVariablesReferenced(vars);
            foreach (Executable ex in this.TrueCode.Concat(this.FalseCode))
            {
                ex.GetAllVariablesReferenced(vars);
            }
        }

        internal override void PerformLocalIdAllocation(VariableIdAllocator varIds, VariableIdAllocPhase phase)
        {
            this.Condition.PerformLocalIdAllocation(varIds, phase);
            if (phase != VariableIdAllocPhase.REGISTER_AND_ALLOC || this.TrueCode.Length == 0 || this.FalseCode.Length == 0)
            {
                foreach (Executable ex in this.TrueCode.Concat(this.FalseCode))
                {
                    ex.PerformLocalIdAllocation(varIds, phase);
                }
            }
            else
            {
                // branch the variable ID allocator. 
                VariableIdAllocator trueVars = varIds.Clone();
                VariableIdAllocator falseVars = varIds.Clone();

                // Go through and register and allocate all variables.
                // The allocated ID's are going to be garbage, but this enforces that they must be
                // declared before being used.
                foreach (Executable ex in this.TrueCode)
                {
                    ex.PerformLocalIdAllocation(trueVars, VariableIdAllocPhase.REGISTER_AND_ALLOC);
                }
                foreach (Executable ex in this.FalseCode)
                {
                    ex.PerformLocalIdAllocation(falseVars, VariableIdAllocPhase.REGISTER_AND_ALLOC);
                }

                // Now that the code is as correct as we can verify, merge the branches back together
                // creating a new set of variable ID's.
                varIds.MergeClonesBack(trueVars, falseVars);

                // Go back through and do another allocation pass and assign the correct variable ID's.
                foreach (Executable ex in this.TrueCode.Concat(this.FalseCode))
                {
                    ex.PerformLocalIdAllocation(varIds, VariableIdAllocPhase.ALLOC);
                }
            }
        }
    }
}
