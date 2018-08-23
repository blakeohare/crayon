using Common;
using Localization;
using System.Collections.Generic;
using System.Linq;

namespace Parser.ParseTree
{
    public class ConstructorDefinition : TopLevelConstruct
    {
        public int FunctionID { get; private set; }
        public Executable[] Code { get; set; }
        public Token[] ArgNames { get; private set; }
        public Expression[] DefaultValues { get; private set; }
        public Expression[] BaseArgs { get; private set; }
        public Token BaseToken { get; private set; }
        public int LocalScopeSize { get; set; }
        public int MinArgCount { get; set; }
        public int MaxArgCount { get; set; }
        public bool IsDefault { get; private set; }
        public AnnotationCollection Annotations { get; set; }

        public ConstructorDefinition(TopLevelConstruct owner, AnnotationCollection annotations)
            : base(null, owner, owner.FileScope)
        {
            this.IsDefault = true;

            this.Code = new Executable[0];
            this.ArgNames = new Token[0];
            this.DefaultValues = new Expression[0];
            this.BaseArgs = new Expression[0];
            this.MaxArgCount = 0;
            this.MinArgCount = 0;
            this.Annotations = annotations;
        }

        public ConstructorDefinition(
            Token constructorToken,
            IList<Token> args,
            IList<Expression> defaultValues,
            IList<Expression> baseArgs,
            IList<Executable> code,
            Token baseToken,
            AnnotationCollection annotations,
            TopLevelConstruct owner)
            : base(constructorToken, owner, owner.FileScope)
        {
            this.IsDefault = false;
            this.ArgNames = args.ToArray();
            this.DefaultValues = defaultValues.ToArray();
            this.BaseArgs = baseArgs.ToArray();
            this.Code = code.ToArray();
            this.BaseToken = baseToken;
            this.Annotations = annotations;

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
            VariableIdAllocator variableIds = new VariableIdAllocator();
            for (int i = 0; i < this.ArgNames.Length; ++i)
            {
                variableIds.RegisterVariable(this.ArgNames[i].Value);
            }

            foreach (Expression arg in this.BaseArgs)
            {
                arg.PerformLocalIdAllocation(parser, variableIds, VariableIdAllocPhase.ALLOC);
            }

            foreach (Executable ex in this.Code)
            {
                ex.PerformLocalIdAllocation(parser, variableIds, VariableIdAllocPhase.REGISTER_AND_ALLOC);
            }
            this.LocalScopeSize = variableIds.Size;
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
    }
}
