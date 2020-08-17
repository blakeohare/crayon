using Parser.Resolver;
using System.Collections.Generic;

namespace Parser.ParseTree
{
    internal class Increment : Expression
    {
        public bool IsIncrement { get; private set; }
        public bool IsPrefix { get; private set; }
        public Expression Root { get; private set; }
        public Token IncrementToken { get; private set; }

        public Increment(Token firstToken, Token incrementToken, bool isIncrement, bool isPrefix, Expression root, Node owner)
            : base(firstToken, owner)
        {
            this.IncrementToken = incrementToken;
            this.IsIncrement = isIncrement;
            this.IsPrefix = isPrefix;
            this.Root = root;
        }

        internal override IEnumerable<Expression> Descendants { get { return new Expression[] { this.Root }; } }

        internal override Expression Resolve(ParserContext parser)
        {
            this.Root = this.Root.Resolve(parser);

            if (!(this.Root is Variable) &&
                !(this.Root is BracketIndex) &&
                !(this.Root is FieldReference) &&
                !(this.Root is DotField))
            {
                throw new ParserException(this.IncrementToken, "Inline increment/decrement operation is not valid for this expression.");
            }
            return this;
        }

        internal override void ResolveVariableOrigins(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase)
        {
            if ((phase & VariableIdAllocPhase.ALLOC) != 0)
            {
                // Despite potentially assigning to a varaible, it does not declare it, so it gets no special treatment.
                this.Root.ResolveVariableOrigins(parser, varIds, phase);
            }
        }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            this.Root = this.Root.ResolveEntityNames(parser);
            return this;
        }

        internal override Expression ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            this.Root = this.Root.ResolveTypes(parser, typeResolver);
            if (!this.Root.ResolvedType.CanAssignToA(ResolvedType.INTEGER))
            {
                throw new ParserException(this.Root, "++ and -- can only be applied to integer values.");
            }

            this.ResolvedType = ResolvedType.INTEGER;
            return this;
        }
    }
}
