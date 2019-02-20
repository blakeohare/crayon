using Parser.Resolver;
using System.Collections.Generic;
using System.Linq;

namespace Parser.ParseTree
{
    public class Lambda : Expression, ICodeContainer
    {
        public Token[] Args { get; private set; }
        public AType[] ArgTypes { get; private set; }
        public VariableId[] ArgVarIds { get; private set; }
        public ResolvedType[] ResolvedArgTypes { get; private set; }
        public Executable[] Code { get; set; }
        public List<Lambda> Lambdas { get; private set; }
        internal VariableScope VariableScope { get; private set; }
        public int LocalScopeSize { get { return this.VariableScope.Size; } }
        public VariableId[] ClosureIds { get { return this.VariableScope.GetClosureIds(); } }
        public HashSet<string> ArgumentNameLookup { get; private set; }

        public Lambda(
            Token firstToken,
            Node owner,
            IList<Token> args,
            IList<AType> argTypes)
            : base(firstToken, owner)
        {
            this.Args = args.ToArray();
            this.ArgTypes = argTypes.ToArray();
            this.Lambdas = new List<Lambda>();
            ((ICodeContainer)owner).Lambdas.Add(this);
            this.ArgumentNameLookup = new HashSet<string>(this.Args.Select(a => a.Value));
        }

        // Descendants is currently used by constant resolution, which cannot contain lambdas.
        internal override IEnumerable<Expression> Descendants { get { return Expression.NO_DESCENDANTS; } }

        internal static void DoVarScopeIdAllocationForLambdaContainer(
            ParserContext parser,
            VariableScope containerScope,
            ICodeContainer container)
        {
            foreach (Lambda lambda in container.Lambdas)
            {
                lambda.ResolveVariableOriginsForInnerCode(parser, containerScope);
            }
        }

        // This is called when the lambda is being resolved as an expression.
        internal override void ResolveVariableOrigins(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase)
        {
        }

        // This is called at the end of the TopLevelEntity's allocation phase and allocates
        // ID's to the lambda's code.
        internal void ResolveVariableOriginsForInnerCode(ParserContext parser, VariableScope scopeFromParent)
        {
            this.VariableScope = VariableScope.CreateClosure(scopeFromParent);
            List<VariableId> argVarIds = new List<VariableId>();
            for (int i = 0; i < this.Args.Length; ++i)
            {
                argVarIds.Add(this.VariableScope.RegisterVariable(this.ArgTypes[i], this.Args[i].Value));
            }
            this.ArgVarIds = argVarIds.ToArray();

            foreach (Executable ex in this.Code)
            {
                ex.ResolveVariableOrigins(parser, this.VariableScope, VariableIdAllocPhase.REGISTER_AND_ALLOC);
            }

            this.VariableScope.FinalizeScopeIds();

            foreach (Lambda lambda in this.Lambdas)
            {
                lambda.ResolveVariableOriginsForInnerCode(parser, this.VariableScope);
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

        internal override Expression ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            int argCount = this.ArgTypes.Length;
            this.ResolvedArgTypes = new ResolvedType[argCount];
            for (int i = 0; i < argCount; ++i)
            {
                ResolvedType argType = typeResolver.ResolveType(this.ArgTypes[i]);
                this.ResolvedArgTypes[i] = argType;

                // TODO: this variableId will not always be non-null when the argument in a lambda is used by a
                // closure of another lambda inside the lambda. I'll have to change my strategy here. Possibly
                // tracking the VariableId's as another Args field.
                VariableId variableId = this.VariableScope.GetVarId(this.Args[i]);
                variableId.ResolvedType = argType;
            }

            foreach (Executable ex in this.Code)
            {
                ex.ResolveTypes(parser, typeResolver);
            }

            // TODO: how do you define the lambda return type in Acrylic? Snoop the nested returns, maybe?
            ResolvedType returnType = ResolvedType.ANY;

            this.ResolvedType = ResolvedType.GetFunctionType(returnType, this.ResolvedArgTypes, 0);

            return this;
        }
    }
}
