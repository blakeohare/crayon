using Common;
using Parser.Localization;
using Parser.Resolver;
using System.Collections.Generic;
using System.Linq;

namespace Parser.ParseTree
{
    internal class ConstructorDefinition : TopLevelEntity, ICodeContainer
    {
        private static readonly Token[] NO_TOKENS = new Token[0];
        private static readonly AType[] NO_TYPES = new AType[0];
        private static readonly Expression[] NO_EXPRESSIONS = new Expression[0];
        private static readonly Executable[] NO_EXECUTABLES = new Executable[0];

        public int FunctionID { get; private set; }
        public Executable[] Code { get; set; }
        public AType[] ArgTypes { get; set; }
        public ResolvedType[] ResolvedArgTypes { get; private set; }
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
        public HashSet<string> ArgumentNameLookup { get; private set; }

        public ConstructorDefinition(ClassDefinition owner, ModifierCollection modifiers, AnnotationCollection annotations)
            : this(null, modifiers, annotations, owner)
        {
            this.IsDefault = true;
        }

        public ConstructorDefinition(
            Token constructorToken,
            ModifierCollection modifiers,
            AnnotationCollection annotations,
            ClassDefinition owner)
            : base(constructorToken, owner, owner.FileScope, modifiers)
        {
            this.IsDefault = false;
            this.Annotations = annotations;
            this.ArgTypes = NO_TYPES;
            this.ArgNames = NO_TOKENS;
            this.DefaultValues = NO_EXPRESSIONS;
            this.MaxArgCount = 0;
            this.MinArgCount = 0;
            this.BaseArgs = NO_EXPRESSIONS;
            this.Code = NO_EXECUTABLES;
            this.Lambdas = new List<Lambda>();
        }

        internal void SetArgs(IList<Token> argNames, IList<Expression> defaultValues, IList<AType> argTypes)
        {
            this.ArgNames = argNames.ToArray();
            this.DefaultValues = defaultValues.ToArray();
            this.ArgTypes = argTypes.ToArray();
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

            this.ArgumentNameLookup = new HashSet<string>(this.ArgNames.Select(a => a.Value));
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

        public VariableId[] ArgLocalIds { get; private set; }

        internal void ResolveVariableOrigins(ParserContext parser)
        {
            VariableScope varScope = VariableScope.NewEmptyScope(this.CompilationScope.IsStaticallyTyped);
            this.ArgLocalIds = new VariableId[this.ArgNames.Length];
            for (int i = 0; i < this.ArgNames.Length; ++i)
            {
                this.ArgLocalIds[i] = varScope.RegisterVariable(this.ArgTypes[i], this.ArgNames[i].Value);
            }

            foreach (Expression arg in this.BaseArgs)
            {
                arg.ResolveVariableOrigins(parser, varScope, VariableIdAllocPhase.ALLOC);
            }

            foreach (Executable ex in this.Code)
            {
                ex.ResolveVariableOrigins(parser, varScope, VariableIdAllocPhase.REGISTER_AND_ALLOC);
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

        internal override void ResolveSignatureTypes(ParserContext parser, TypeResolver typeResolver)
        {
            this.ResolvedArgTypes = typeResolver.ResolveTypes(this.ArgTypes);
        }

        internal override void EnsureModifierAndTypeSignatureConsistency()
        {
            bool isStatic = this.Modifiers.HasStatic;
            ClassDefinition classDef = (ClassDefinition)this.Owner;
            ClassDefinition baseClass = classDef.BaseClass;
            bool hasBaseClass = baseClass != null;

            if (!isStatic && classDef.Modifiers.HasStatic && !this.IsDefault)
            {
                throw new ParserException(this, "Cannot have a non-static constructor in a static class.");
            }

            if (this.BaseToken != null)
            {
                if (isStatic) throw new ParserException(this.BaseToken, "Cannot invoke the base constructor from a static constructor.");
                if (!hasBaseClass) throw new ParserException(this.BaseToken, "There is no base class for this constructor to invoke.");
            }

            if (hasBaseClass && !Node.IsAccessAllowed(this, baseClass.Constructor))
            {
                throw new ParserException(this, "Cannot invoke the base constructor due to its access modifier.");
            }
        }

        internal override void ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            for (int i = 0; i < this.ArgNames.Length; ++i)
            {
                this.ArgLocalIds[i].ResolvedType = typeResolver.ResolveType(this.ArgTypes[i]);
            }

            foreach (Expression defaultArg in this.DefaultValues)
            {
                if (defaultArg != null)
                {
                    defaultArg.ResolveTypes(parser, typeResolver);
                }
            }

            ClassDefinition cd = (ClassDefinition)this.Owner;
            if (this.Modifiers.HasStatic)
            {
                // already verified that there's no base constructor invocation
            }
            else if (cd.BaseClass != null)
            {
                ConstructorDefinition baseConstructor = cd.BaseClass.Constructor;
                ResolvedType[] baseConstructorArgTypes = baseConstructor == null
                    ? new ResolvedType[0]
                    : baseConstructor.ResolvedArgTypes;
                Expression[] baseConstructorDefaultValues = baseConstructor == null
                    ? new Expression[0]
                    : baseConstructor.DefaultValues;
                int optionalArgCount = FunctionCall.CountOptionalArgs(baseConstructorDefaultValues);
                int maxArgCount = baseConstructorArgTypes.Length;
                int minArgCount = maxArgCount - optionalArgCount;
                if (this.BaseArgs.Length < minArgCount || this.BaseArgs.Length > maxArgCount)
                {
                    if (this.FirstToken == null)
                    {
                        throw new ParserException(cd.FirstToken, "The class '" + cd.NameToken.Value + "' cannot have an implicit constructor since it needs to pass arguments to the base constructor of '" + cd.BaseClass.NameToken.Value + "'.");
                    }

                    if (this.BaseToken == null)
                    {
                        throw new ParserException(this, "You are missing a call to the base constructor, which is required because class '" + baseConstructor.ClassOwner.NameToken.Value + "' requires arguments for its constructor.");
                    }

                    throw new ParserException(this, "Incorrect number of arguments passed to base constructor.");
                }

                for (int i = 0; i < this.BaseArgs.Length; ++i)
                {
                    this.BaseArgs[i] = this.BaseArgs[i].ResolveTypes(parser, typeResolver);

                    ResolvedType actualType = this.BaseArgs[i].ResolvedType;
                    ResolvedType expectedType = baseConstructorArgTypes[i];
                    if (!actualType.CanAssignToA(expectedType))
                    {
                        throw new ParserException(this.BaseArgs[i], "Argument is incorrect type.");
                    }
                    if (actualType == ResolvedType.ANY && expectedType != ResolvedType.OBJECT && expectedType != ResolvedType.ANY)
                    {
                        this.BaseArgs[i] = new Cast(this.BaseArgs[i].FirstToken, expectedType, this.BaseArgs[i], this, false);
                    }
                }
            }

            foreach (Executable line in this.Code)
            {
                line.ResolveTypes(parser, typeResolver);
            }
        }
    }
}
