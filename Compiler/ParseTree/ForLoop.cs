﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
    internal class ForLoop : Executable
    {
        public Executable[] Init { get; private set; }
        public Expression Condition { get; private set; }
        public Executable[] Step { get; private set; }
        public Executable[] Code { get; private set; }

        public ForLoop(Token forToken, IList<Executable> init, Expression condition, IList<Executable> step, IList<Executable> code, Executable owner)
            : base(forToken, owner)
        {
            this.Init = init.ToArray();
            this.Condition = condition ?? new BooleanConstant(forToken, true, owner);
            this.Step = step.ToArray();
            this.Code = code.ToArray();
        }

        internal override IList<Executable> Resolve(Parser parser)
        {
            this.Init = Resolve(parser, this.Init).ToArray();
            this.Condition = this.Condition.Resolve(parser);
            this.Step = Resolve(parser, this.Step).ToArray();
            this.Code = Resolve(parser, this.Code).ToArray();

            return Listify(this);
        }

        internal override void GetAllVariableNames(Dictionary<string, bool> lookup)
        {
            foreach (Executable init in this.Init)
            {
                init.GetAllVariableNames(lookup);
            }

            foreach (Executable step in this.Step)
            {
                step.GetAllVariableNames(lookup);
            }

            foreach (Executable line in this.Code)
            {
                line.GetAllVariableNames(lookup);
            }
        }

        internal override Executable ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
        {
            this.BatchExecutableNameResolver(parser, lookup, imports, this.Init);
            this.Condition = this.Condition.ResolveNames(parser, lookup, imports);
            this.BatchExecutableNameResolver(parser, lookup, imports, this.Step);
            this.BatchExecutableNameResolver(parser, lookup, imports, this.Code);
            return this;
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars)
        {
            this.Condition.GetAllVariablesReferenced(vars);
            foreach (Executable ex in this.Init.Concat(this.Step).Concat(this.Code))
            {
                ex.GetAllVariablesReferenced(vars);
            }
        }

        internal override void PerformLocalIdAllocation(VariableIdAllocator varIds, VariableIdAllocPhase phase)
        {
            bool register = (phase & VariableIdAllocPhase.REGISTER) != 0;
            bool alloc = (phase & VariableIdAllocPhase.ALLOC) != 0;
            bool both = register && alloc;

            foreach (Executable ex in this.Init)
            {
                ex.PerformLocalIdAllocation(varIds, phase);
            }

            this.Condition.PerformLocalIdAllocation(varIds, phase);

            if (both)
            {
                foreach (Executable ex in this.Code.Concat(this.Step))
                {
                    ex.PerformLocalIdAllocation(varIds, VariableIdAllocPhase.REGISTER);
                }

                foreach (Executable ex in this.Code.Concat(this.Step))
                {
                    ex.PerformLocalIdAllocation(varIds, VariableIdAllocPhase.ALLOC);
                }
            }
            else
            {
                foreach (Executable ex in this.Code.Concat(this.Step))
                {
                    ex.PerformLocalIdAllocation(varIds, phase);
                }
            }
        }

        internal override Executable PastelResolve(Parser parser)
        {
            this.Init = Executable.PastelResolveExecutables(parser, this.Init);
            this.Condition = this.Condition.PastelResolve(parser);
            this.Step = Executable.PastelResolveExecutables(parser, this.Step);
            this.Code = Executable.PastelResolveExecutables(parser, this.Code);
            return this;
        }
    }
}
