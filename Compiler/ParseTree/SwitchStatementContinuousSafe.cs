using System;
using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
    internal class SwitchStatementContinuousSafe : Executable
    {
        private Dictionary<int, Executable[]> codeByCondition;
        public Expression Condition { get; private set; }
        public SwitchStatement OriginalSwitchStatement { get; private set; }

        public SwitchStatementContinuousSafe(SwitchStatement switchStatement, Executable owner)
            : base(switchStatement.FirstToken, owner)
        {
            this.OriginalSwitchStatement = switchStatement;
            this.Condition = switchStatement.Condition;
            this.codeByCondition = new Dictionary<int, Executable[]>();
            foreach (SwitchStatement.Chunk chunk in switchStatement.Chunks)
            {
                this.codeByCondition.Add(((IntegerConstant)chunk.Cases[0]).Value, chunk.Code);
            }
        }

        internal override IList<Executable> Resolve(Parser parser)
        {
            bool removeBreaks = parser.RemoveBreaksFromSwitch;

            int[] keys = codeByCondition.Keys.ToArray(); // iterate over a copy of keys so you don't modify the enumerable while iterating.
            foreach (int key in keys)
            {
                codeByCondition[key] = Executable.RemoveBreaksForElifedSwitch(removeBreaks, codeByCondition[key]);
            }

            return Listify(this);
        }

        internal override Executable ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
        {
            throw new InvalidOperationException();
        }
        
        public SearchTree GenerateSearchTree()
        {
            int[] keys = this.codeByCondition.Keys.ToArray();
            int min = keys.First<int>();
            int max = keys.First<int>();
            foreach (int key in keys)
            {
                if (key < min) min = key;
                if (key > max) max = key;
            }

            return SwitchStatementContinuousSafe.GenerateSearchTreeCommon(this.codeByCondition, min, max);
        }

        internal static SearchTree GenerateSearchTreeCommon(Dictionary<int, Executable[]> codeByLookup, int min, int max)
        {
            if (min == max)
            {
                return new SearchTree() { Code = codeByLookup[min] };
            }

            if (min + 1 == max)
            {
                SearchTree left = new SearchTree() { Code = codeByLookup[min] };
                SearchTree right = new SearchTree() { Code = codeByLookup[max] };
                return new SearchTree() { LessThanThis = max, Left = left, Right = right };
            }

            int mid = (min + max + 1) / 2;

            return new SearchTree()
            {
                LessThanThis = mid,
                Left = GenerateSearchTreeCommon(codeByLookup, min, mid - 1),
                Right = GenerateSearchTreeCommon(codeByLookup, mid, max)
            };
        }

        public class SearchTree
        {
            public Executable[] Code { get; set; }
            public int LessThanThis { get; set; }
            public SearchTree Left { get; set; }
            public SearchTree Right { get; set; }
        }

        internal override void GetAllVariableNames(Dictionary<string, bool> lookup)
        {
            this.OriginalSwitchStatement.GetAllVariableNames(lookup);
        }
        
        internal override void PerformLocalIdAllocation(VariableIdAllocator varIds, VariableIdAllocPhase phase)
        {
            throw new InvalidOperationException(); // translate mode only
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars)
        {
            this.Condition.GetAllVariablesReferenced(vars);
            foreach (Executable[] chunks in this.codeByCondition.Values)
            {
                foreach (Executable chunk in chunks)
                {
                    chunk.GetAllVariablesReferenced(vars);
                }
            }
        }
    }
}
