using System.Collections.Generic;
using System.Linq;

namespace Parser.ParseTree
{
    public class Lambda : Expression, ICodeContainer
    {
        public Token[] Args { get; private set; }
        public Executable[] Code { get; private set; }
        public List<Lambda> Lambdas { get; private set; }

        public override bool CanAssignTo { get { return false; } }

        public Lambda(
            Token firstToken,
            Node owner,
            IList<Expression> args,
            IList<Executable> code)
            : base(firstToken, owner)
        {
            // TODO: ugh, change this at parse-time.
            this.Args = args.Select(arg => arg.FirstToken).ToArray();
            this.Code = code.ToArray();
            this.Lambdas = new List<Lambda>();
        }

        internal static void DoVarScopeIdAllocationForLambdaContainer(
            ParserContext parser,
            VariableScope containerScope,
            ICodeContainer container)
        {
            foreach (Lambda lambda in container.Lambdas)
            {
                lambda.AllocateLocalScopeIds(parser, containerScope);
            }
        }

        // This is called when the lambda is being resolved as an expression.
        internal override void PerformLocalIdAllocation(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase)
        {
            ((ICodeContainer)this.Owner).Lambdas.Add(this);
        }

        // This is called at the end of the TopLevelEntity's allocation phase and allocates
        // ID's to the lambda's code.
        internal void AllocateLocalScopeIds(ParserContext parser, VariableScope scopeFromParent)
        {
            VariableScope varScope = VariableScope.CreateClosure(scopeFromParent);
            for (int i = 0; i < this.Args.Length; ++i)
            {
                varScope.RegisterVariable(this.Args[i].Value);
            }

            foreach (Executable ex in this.Code)
            {
                ex.PerformLocalIdAllocation(parser, varScope, VariableIdAllocPhase.REGISTER_AND_ALLOC);
            }

            foreach (Lambda lambda in this.Lambdas)
            {
                lambda.AllocateLocalScopeIds(parser, varScope);
            }
        }

        internal override Expression Resolve(ParserContext parser)
        {
            List<Executable> newCode = new List<Executable>();
            foreach (Executable ex in this.Code)
            {
                newCode.AddRange(ex.Resolve(parser));
            }
            this.Code = newCode.ToArray();

            return this;
        }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            for (int i = 0; i < this.Code.Length; ++i)
            {
                this.Code[i] = this.Code[i].ResolveEntityNames(parser);
            }

            return this;
        }
    }
}
