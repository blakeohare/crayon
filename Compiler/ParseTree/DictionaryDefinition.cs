using System;
using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
    internal class DictionaryDefinition : Expression
    {
        internal override Expression PastelResolve(Parser parser)
        {
            throw new NotImplementedException();
        }

        public override bool CanAssignTo { get { return false; } }

        public Expression[] Keys { get; private set; }
        public Expression[] Values { get; private set; }

        public DictionaryDefinition(Token braceToken, IList<Expression> keys, IList<Expression> values, Executable owner)
            : base(braceToken, owner)
        {
            this.Keys = keys.ToArray();
            this.Values = values.ToArray();
        }

        internal override Expression Resolve(Parser parser)
        {
            // Iterate through KVP in parallel so that errors will get reported in the preferred order.

            // TODO: verify no duplicate keys and keys are all of correct type (amongst the 
            // keys that can be resolved into constants, at least).
            for (int i = 0; i < this.Keys.Length; ++i)
            {
                this.Keys[i] = this.Keys[i].Resolve(parser);
                this.Values[i] = this.Values[i].Resolve(parser);
            }
            return this;
        }

        internal override void PerformLocalIdAllocation(VariableIdAllocator varIds, VariableIdAllocPhase phase)
        {
            if ((phase & VariableIdAllocPhase.ALLOC) != 0)
            {
                // Iterate through KVP in parallel so that errors will get reported in the preferred order.
                for (int i = 0; i < this.Keys.Length; ++i)
                {
                    this.Keys[i].PerformLocalIdAllocation(varIds, phase);
                    this.Values[i].PerformLocalIdAllocation(varIds, phase);
                }
            }
        }

        internal override Expression ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
        {
            this.BatchExpressionNameResolver(parser, lookup, imports, this.Keys);
            this.BatchExpressionNameResolver(parser, lookup, imports, this.Values);
            return this;
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars)
        {
            for (int i = 0; i < this.Keys.Length; ++i)
            {
                this.Keys[i].GetAllVariablesReferenced(vars);
                this.Values[i].GetAllVariablesReferenced(vars);
            }
        }
    }
}
