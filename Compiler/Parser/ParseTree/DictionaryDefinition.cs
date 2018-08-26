using System;
using System.Collections.Generic;
using System.Linq;
using Common;

namespace Parser.ParseTree
{
    public class DictionaryDefinition : Expression
    {
        public override bool CanAssignTo { get { return false; } }

        public Expression[] Keys { get; private set; }
        public Expression[] Values { get; private set; }

        public DictionaryDefinition(Token braceToken, IList<Expression> keys, IList<Expression> values, Node owner)
            : base(braceToken, owner)
        {
            this.Keys = keys.ToArray();
            this.Values = values.ToArray();
        }

        internal override Expression Resolve(ParserContext parser)
        {
            // Iterate through KVP in parallel so that errors will get reported in the preferred order.

            TODO.VerifyNoDuplicateKeysInDictionaryDefinition();
            TODO.VerifyAllDictionaryKeysAreCorrectType(); // amongst the keys that can be resolved into constants, at least.

            for (int i = 0; i < this.Keys.Length; ++i)
            {
                this.Keys[i] = this.Keys[i].Resolve(parser);
                this.Values[i] = this.Values[i].Resolve(parser);
            }
            return this;
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase)
        {
            if ((phase & VariableIdAllocPhase.ALLOC) != 0)
            {
                // Iterate through KVP in parallel so that errors will get reported in the preferred order.
                for (int i = 0; i < this.Keys.Length; ++i)
                {
                    this.Keys[i].PerformLocalIdAllocation(parser, varIds, phase);
                    this.Values[i].PerformLocalIdAllocation(parser, varIds, phase);
                }
            }
        }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            this.BatchExpressionEntityNameResolver(parser, this.Keys);
            this.BatchExpressionEntityNameResolver(parser, this.Values);
            return this;
        }
    }
}
