using Common;
using Localization;
using System.Collections.Generic;
using System.Linq;

namespace Parser.ParseTree
{
    public class ConstructorDefinition : TopLevelEntity, ICodeContainer
    {
        private static readonly Token[] NO_TOKENS = new Token[0];
        private static readonly Expression[] NO_EXPRESSIONS = new Expression[0];
        private static readonly Executable[] NO_EXECUTABLES = new Executable[0];

        public int FunctionID { get; private set; }
        public Executable[] Code { get; set; }
        public Token[] ArgNames { get; private set; }
        public Expression[] DefaultValues { get; private set; }
        public Expression[] BaseArgs { get; private set; }
        public Token BaseToken { get; set; }
        public int LocalScopeSize { get; set; }
        public int MinArgCount { get; set; }
        public int MaxArgCount { get; set; }
        public bool IsDefault { get; private set; }
        public AnnotationCollection Annotations { get; set; }
        public List<Lambda> Lambdas { get; private set; }

        public ConstructorDefinition(ClassDefinition owner, AnnotationCollection annotations)
            : this(null, annotations, owner)
        {
            this.IsDefault = true;
        }

        public ConstructorDefinition(
            Token constructorToken,
            AnnotationCollection annotations,
            ClassDefinition owner)
            : base(constructorToken, owner, owner.FileScope)
        {
            this.IsDefault = false;
            this.Annotations = annotations;
            this.ArgNames = NO_TOKENS;
            this.DefaultValues = NO_EXPRESSIONS;
            this.MaxArgCount = 0;
            this.MinArgCount = 0;
            this.BaseArgs = NO_EXPRESSIONS;
            this.Code = NO_EXECUTABLES;
            this.Lambdas = new List<Lambda>();
        }

        internal void SetArgs(IList<Token> argNames, IList<Expression> defaultValues)
        {
            this.ArgNames = argNames.ToArray();
            this.DefaultValues = defaultValues.ToArray();
            TODO.VerifyDefaultArgumentsAreAtTheEnd();

            this.MaxArgCount = this.ArgNames.Length;
            int minArgCount = 0;
            for (int i = 0; i < this.ArgNames.Length; ++i)
            {
                if (this.DefaultValues[i] == null)
                {
                    minArgCount++;
                }
                else
                {
                    break;
                }
            }
            this.MinArgCount = minArgCount;
        }

        internal void SetBaseArgs(IList<Expression> baseArgs)
        {
            this.BaseArgs = baseArgs.ToArray();
        }

        internal void SetCode(IList<Executable> code)
        {
            this.Code = code.ToArray();
        }

        public override string GetFullyQualifiedLocalizedName(Locale locale)
        {
            // This shouldn't be called.
            throw new System.Exception();
        }

        public void ResolvePublic(ParserContext parser)
        {
            this.Resolve(parser);
        }

        internal override void Resolve(ParserContext parser)
        {
            parser.ValueStackDepth = 0;

            this.FunctionID = parser.GetNextFunctionId();

            for (int i = 0; i < this.ArgNames.Length; ++i)
            {
                if (this.DefaultValues[i] != null)
                {
                    this.DefaultValues[i] = this.DefaultValues[i].Resolve(parser);
                }
            }

            for (int i = 0; i < this.BaseArgs.Length; ++i)
            {
                this.BaseArgs[i] = this.BaseArgs[i].Resolve(parser);
            }

            List<Executable> code = new List<Executable>();
            foreach (Executable line in this.Code)
            {
                code.AddRange(line.Resolve(parser));
            }
            this.Code = code.ToArray();
        }

        internal void AllocateLocalScopeIds(ParserContext parser)
        {
            VariableScope varScope = VariableScope.NewEmptyScope();
            for (int i = 0; i < this.ArgNames.Length; ++i)
            {
                varScope.RegisterVariable(this.ArgNames[i].Value);
            }

            foreach (Expression arg in this.BaseArgs)
            {
                arg.PerformLocalIdAllocation(parser, varScope, VariableIdAllocPhase.ALLOC);
            }

            foreach (Executable ex in this.Code)
            {
                ex.PerformLocalIdAllocation(parser, varScope, VariableIdAllocPhase.REGISTER_AND_ALLOC);
            }

            Lambda.DoVarScopeIdAllocationForLambdaContainer(parser, varScope, this);

            varScope.FinalizeScopeIds();

            this.LocalScopeSize = varScope.Size;
        }

        internal override void ResolveEntityNames(ParserContext parser)
        {
            parser.CurrentCodeContainer = this;
            if (this.DefaultValues.Length > 0)
            {
                this.BatchExpressionEntityNameResolver(parser, this.DefaultValues);
            }

            if (this.BaseArgs.Length > 0)
            {
                this.BatchExpressionEntityNameResolver(parser, this.BaseArgs);
            }
            this.BatchExecutableEntityNameResolver(parser, this.Code);
            parser.CurrentCodeContainer = null;
        }

        internal override void ResolveTypes(ParserContext parser)
        {
        }
    }
}
