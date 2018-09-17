using Parser.Resolver;
using System.Collections.Generic;

namespace Parser.ParseTree
{
    public class Assignment : Executable
    {
        public Expression Target { get; private set; }
        public Expression Value { get; private set; }
        public Token OpToken { get; private set; }
        public Ops Op { get; private set; }
        public Variable TargetAsVariable { get { return this.Target as Variable; } }
        public AType NullableTypeDeclaration { get; private set; }
        public ResolvedType ResolvedNullableTypeDeclaration { get; private set; }

        public Assignment(Expression target, AType nullableDeclarationType, Token assignmentOpToken, Ops opOverride, Expression assignedValue, Node owner)
            : this(true, target, nullableDeclarationType, assignmentOpToken, opOverride, assignedValue, owner)
        { }

        public Assignment(Expression target, AType nullableDeclarationType, Token assignmentOpToken, Expression assignedValue, Node owner)
            : this(true, target, nullableDeclarationType, assignmentOpToken, GetOpFromToken(assignmentOpToken), assignedValue, owner)
        { }

        private Assignment(bool nonAmbiguousIgnored, Expression target, AType nullableTypeDeclaration, Token assignmentOpToken, Ops op, Expression assignedValue, Node owner)
            : base(target.FirstToken, owner)
        {
            this.Target = target;
            this.OpToken = assignmentOpToken;
            this.Op = op;
            this.Value = assignedValue;
            this.NullableTypeDeclaration = nullableTypeDeclaration;
        }

        private static Ops GetOpFromToken(Token token)
        {
            switch (token.Value)
            {
                case "+=": return Ops.ADDITION;
                case "&=": return Ops.BITWISE_AND;
                case "|=": return Ops.BITWISE_OR;
                case "^=": return Ops.BITWISE_XOR;
                case "<<=": return Ops.BIT_SHIFT_LEFT;
                case ">>=": return Ops.BIT_SHIFT_RIGHT;
                case "/=": return Ops.DIVISION;
                case "=": return Ops.EQUALS;
                case "**=": return Ops.EXPONENT;
                case "%=": return Ops.MODULO;
                case "*=": return Ops.MULTIPLICATION;
                case "-=": return Ops.SUBTRACTION;
                case "++": return Ops.ADDITION;
                case "--": return Ops.SUBTRACTION;
                default: throw new ParserException(token, "Unrecognized assignment op: '" + token.Value + "'");
            }
        }

        internal override IList<Executable> Resolve(ParserContext parser)
        {
            this.Target = this.Target.Resolve(parser);

            if (this.Target is Variable)
            {
                this.Target = this.Target.Resolve(parser);
            }
            else if (this.Target is BracketIndex)
            {
                BracketIndex bi = this.Target as BracketIndex;
                bi.Root = bi.Root.Resolve(parser);
                bi.Index = bi.Index.Resolve(parser);
            }
            else if (this.Target is DotField)
            {
                DotField ds = this.Target as DotField;
                ds.Root = ds.Root.Resolve(parser);
            }

            this.Value = this.Value.Resolve(parser);

            return Listify(this);
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase)
        {
            this.Value.PerformLocalIdAllocation(parser, varIds, phase);

            if ((phase & VariableIdAllocPhase.REGISTER) != 0)
            {
                bool isVariableAssigned =
                    // A variable is considered declared if the target is a variable and = is used instead of something like +=
                    this.Target is Variable &&
                    this.Op == Ops.EQUALS;

                if (isVariableAssigned)
                {
                    if (parser.RequireExplicitVarDeclarations)
                    {
                        if (this.NullableTypeDeclaration != null)
                        {
                            varIds.RegisterVariable(this.NullableTypeDeclaration, this.TargetAsVariable.Name, false);
                        }
                    }
                    else
                    {
                        varIds.RegisterVariable(AType.Any(this.FirstToken), this.TargetAsVariable.Name);
                    }
                }
            }

            this.Target.PerformLocalIdAllocation(parser, varIds, phase);
        }

        internal override Executable ResolveEntityNames(ParserContext parser)
        {
            this.Target = this.Target.ResolveEntityNames(parser);
            this.Value = this.Value.ResolveEntityNames(parser);

            if (!this.Target.CanAssignTo)
            {
                throw new ParserException(this.Target, "Cannot use assignment on this.");
            }

            return this;
        }

        internal override void ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            throw new System.NotImplementedException();
        }
    }
}
