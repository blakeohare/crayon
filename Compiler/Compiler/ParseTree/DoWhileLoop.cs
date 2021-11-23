using Parser.Resolver;
using System.Collections.Generic;
using System.Linq;

namespace Parser.ParseTree
{
    internal class DoWhileLoop : Executable
    {
        public Executable[] Code { get; private set; }
        public Expression Condition { get; private set; }

        public DoWhileLoop(Token doToken, IList<Executable> code, Expression condition, Node owner)
            : base(doToken, owner)
        {
            this.Code = code.ToArray();
            this.Condition = condition;
        }

        internal override IList<Executable> Resolve(ParserContext parser)
        {
            this.Code = Resolve(parser, this.Code).ToArray();

            this.Condition = this.Condition.Resolve(parser);

            return Listify(this);
        }

        internal override Executable ResolveEntityNames(ParserContext parser)
        {
            this.BatchExecutableEntityNameResolver(parser, this.Code);
            this.Condition = this.Condition.ResolveEntityNames(parser);
            return this;
        }

        internal override void ResolveVariableOrigins(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase)
        {
            if (phase != VariableIdAllocPhase.REGISTER_AND_ALLOC)
            {
                foreach (Executable ex in this.Code)
                {
                    ex.ResolveVariableOrigins(parser, varIds, phase);
                }
                this.Condition.ResolveVariableOrigins(parser, varIds, phase);
            }
            else
            {
                foreach (Executable ex in this.Code)
                {
                    ex.ResolveVariableOrigins(parser, varIds, VariableIdAllocPhase.REGISTER);
                }
                foreach (Executable ex in this.Code)
                {
                    ex.ResolveVariableOrigins(parser, varIds, VariableIdAllocPhase.ALLOC);
                }
                this.Condition.ResolveVariableOrigins(parser, varIds, VariableIdAllocPhase.ALLOC);
            }
        }

        internal override void ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            foreach (Executable ex in this.Code)
            {
                ex.ResolveTypes(parser, typeResolver);
            }

            this.Condition = this.Condition.ResolveTypes(parser, typeResolver);
            if (!this.Condition.ResolvedType.CanAssignToA(ResolvedType.BOOLEAN))
            {
                throw new ParserException(this.Condition, "Can only use a boolean for while loop conditions.");
            }
        }
    }
}
