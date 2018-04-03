using System;
using System.Collections.Generic;

namespace Parser.ParseTree
{
    public class Increment : Expression
    {
        internal override Expression PastelResolve(ParserContext parser)
        {
            throw new NotImplementedException();
        }

        public override bool CanAssignTo { get { return false; } }

        public bool IsIncrement { get; private set; }
        public bool IsPrefix { get; private set; }
        public Expression Root { get; private set; }
        public Token IncrementToken { get; private set; }

        public Increment(Token firstToken, Token incrementToken, bool isIncrement, bool isPrefix, Expression root, TopLevelConstruct owner)
            : base(firstToken, owner)
        {
            this.IncrementToken = incrementToken;
            this.IsIncrement = isIncrement;
            this.IsPrefix = isPrefix;
            this.Root = root;
        }

        internal override Expression Resolve(ParserContext parser)
        {
            this.Root = this.Root.Resolve(parser);

            if (!(this.Root is Variable) &&
                !(this.Root is BracketIndex) &&
                !(this.Root is FieldReference) &&
                !(this.Root is DotStep))
            {
                throw new ParserException(this.IncrementToken, "Inline increment/decrement operation is not valid for this expression.");
            }
            return this;
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableIdAllocator varIds, VariableIdAllocPhase phase)
        {
            if ((phase & VariableIdAllocPhase.ALLOC) != 0)
            {
                // Despite potentially assigning to a varaible, it does not declare it, so it gets no special treatment.
                this.Root.PerformLocalIdAllocation(parser, varIds, phase);
            }
        }

        internal override Expression ResolveNames(ParserContext parser)
        {
            this.Root = this.Root.ResolveNames(parser);
            return this;
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars)
        {
            this.Root.GetAllVariablesReferenced(vars);
        }
    }
}
