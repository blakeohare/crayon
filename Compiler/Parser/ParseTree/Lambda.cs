using System.Collections.Generic;
using System.Linq;

namespace Parser.ParseTree
{
    public class Lambda : Expression, ICodeContainer
    {
        public Token[] Args { get; private set; }
        public Executable[] Code { get; private set; }
        public List<Lambda> Lambdas { get; private set; }
        internal VariableScope VariableScope { get; private set; }
        public int LocalScopeSize { get { return this.VariableScope.Size; } }
        public VariableId[] ClosureIds { get { return this.VariableScope.GetClosureIds(); } }

        public override bool CanAssignTo { get { return false; } }

        public Lambda(
            Token firstToken,
            Node owner,
            IList<Token> args,
            IList<Executable> code)
            : base(firstToken, owner)
        {
            this.Args = args.ToArray();
            this.Code = code.ToArray();
            this.Lambdas = new List<Lambda>();
            ((ICodeContainer)owner).Lambdas.Add(this);
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
        }

        // This is called at the end of the TopLevelEntity's allocation phase and allocates
        // ID's to the lambda's code.
        internal void AllocateLocalScopeIds(ParserContext parser, VariableScope scopeFromParent)
        {
            this.VariableScope = VariableScope.CreateClosure(scopeFromParent);
            for (int i = 0; i < this.Args.Length; ++i)
            {
                this.VariableScope.RegisterVariable(this.Args[i].Value);
            }

            foreach (Executable ex in this.Code)
            {
                ex.PerformLocalIdAllocation(parser, this.VariableScope, VariableIdAllocPhase.REGISTER_AND_ALLOC);
            }

            foreach (Lambda lambda in this.Lambdas)
            {
                lambda.AllocateLocalScopeIds(parser, this.VariableScope);
            }
        }

        internal override Expression Resolve(ParserContext parser)
        {
            List<Executable> newCode = new List<Executable>();
            foreach (Executable ex in this.Code)
            {
                newCode.AddRange(ex.Resolve(parser));
            }

            this.Code = Executable.EnsureBlockReturns(newCode.ToArray(), this);

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
