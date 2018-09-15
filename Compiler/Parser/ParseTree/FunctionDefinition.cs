using Localization;
using System.Collections.Generic;
using System.Linq;

namespace Parser.ParseTree
{
    public class FunctionDefinition : TopLevelEntity, ICodeContainer
    {
        public int FunctionID { get; set; }
        public Token NameToken { get; private set; }
        public bool IsStaticMethod { get; private set; }
        public AType[] ArgTypes { get; set; }
        public Token[] ArgNames { get; set; }
        public Expression[] DefaultValues { get; set; }
        private int[] argVarIds = null;
        public Executable[] Code { get; set; }
        public AnnotationCollection Annotations { get; set; }
        public int LocalScopeSize { get; set; }
        public int FinalizedPC { get; set; }
        public int MemberID { get; set; }
        public List<Lambda> Lambdas { get; private set; }
        private Dictionary<Locale, string> namesByLocale = null;

        public FunctionDefinition(
            Token functionToken,
            TopLevelEntity nullableOwner,
            bool isStaticMethod,
            Token nameToken,
            AnnotationCollection annotations,
            FileScope fileScope)
            : base(functionToken, nullableOwner, fileScope)
        {
            this.IsStaticMethod = isStaticMethod;
            this.NameToken = nameToken;
            this.Annotations = annotations;
            this.MemberID = -1;
            this.Lambdas = new List<Lambda>();
        }

        public override string GetFullyQualifiedLocalizedName(Locale locale)
        {
            if (this.namesByLocale == null) this.namesByLocale = this.Annotations.GetNamesByLocale(1);
            string name = this.namesByLocale.ContainsKey(locale) ? this.namesByLocale[locale] : this.NameToken.Value;
            if (this.TopLevelEntity != null)
            {
                name = this.TopLevelEntity.GetFullyQualifiedLocalizedName(locale) + "." + name;
            }
            return name;
        }

        public int[] ArgVarIDs
        {
            get
            {
                if (this.argVarIds == null)
                {
                    this.argVarIds = new int[this.ArgNames.Length];
                }
                return this.argVarIds;
            }
        }

        internal override void Resolve(ParserContext parser)
        {
            parser.ValueStackDepth = 0;

            this.FunctionID = parser.GetNextFunctionId();

            for (int i = 0; i < this.DefaultValues.Length; ++i)
            {
                if (this.DefaultValues[i] != null)
                {
                    this.DefaultValues[i] = this.DefaultValues[i].Resolve(parser);
                }
            }

            this.Code = Executable.Resolve(parser, this.Code).ToArray();
            this.Code = Executable.EnsureBlockReturns(this.Code, this);
        }

        internal override void ResolveEntityNames(ParserContext parser)
        {
            parser.CurrentCodeContainer = this;
            this.BatchExpressionEntityNameResolver(parser, this.DefaultValues);
            this.BatchExecutableEntityNameResolver(parser, this.Code);
            parser.CurrentCodeContainer = null;
        }

        internal override void ResolveTypes(ParserContext parser)
        {
        }

        internal void AllocateLocalScopeIds(ParserContext parser)
        {
            VariableScope varScope = VariableScope.NewEmptyScope();
            for (int i = 0; i < this.ArgNames.Length; ++i)
            {
                varScope.RegisterVariable(this.ArgNames[i].Value);
            }

            foreach (Executable ex in this.Code)
            {
                ex.PerformLocalIdAllocation(parser, varScope, VariableIdAllocPhase.REGISTER_AND_ALLOC);
            }

            Lambda.DoVarScopeIdAllocationForLambdaContainer(parser, varScope, this);

            varScope.FinalizeScopeIds();

            this.LocalScopeSize = varScope.Size;
        }
    }
}
