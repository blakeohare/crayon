using Localization;
using Parser.Resolver;
using System.Collections.Generic;
using System.Linq;

namespace Parser.ParseTree
{
    public class FunctionDefinition : TopLevelEntity, ICodeContainer
    {
        public int FunctionID { get; set; }
        public Token NameToken { get; private set; }
        public AType ReturnType { get; set; }
        public ResolvedType ResolvedReturnType { get; set; }
        public AType[] ArgTypes { get; set; }
        public ResolvedType[] ResolvedArgTypes { get; set; }
        public Token[] ArgNames { get; set; }
        public VariableId[] ArgLocalIds { get; private set; }
        public Expression[] DefaultValues { get; set; }
        public Executable[] Code { get; set; }
        public AnnotationCollection Annotations { get; set; }
        public int LocalScopeSize { get; set; }
        public int FinalizedPC { get; set; }
        public int MemberID { get; set; }
        public List<Lambda> Lambdas { get; private set; }
        private Dictionary<Locale, string> namesByLocale = null;
        public VariableId[] Locals { get; private set; }

        public FunctionDefinition(
            Token functionToken,
            AType returnType,
            TopLevelEntity nullableOwner,
            Token nameToken,
            ModifierCollection modifiers,
            AnnotationCollection annotations,
            FileScope fileScope)
            : base(functionToken, nullableOwner, fileScope, modifiers)
        {
            this.ReturnType = returnType;
            this.NameToken = nameToken;
            this.Annotations = annotations;
            this.MemberID = -1;
            this.Lambdas = new List<Lambda>();
        }

        private int minArgCount = -1;
        public int MinArgCount
        {
            get
            {
                if (this.minArgCount == -1)
                {
                    this.minArgCount = this.DefaultValues.Count(v => v == null);
                }
                return this.minArgCount;
            }
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

        internal override void ResolveSignatureTypes(ParserContext parser, TypeResolver typeResolver)
        {
            this.ResolvedReturnType = typeResolver.ResolveType(this.ReturnType);

            int argsLength = this.ArgNames.Length;
            this.ResolvedArgTypes = new ResolvedType[argsLength];
            for (int i = 0; i < argsLength; ++i)
            {
                ResolvedType rType = typeResolver.ResolveType(this.ArgTypes[i]);
                this.ResolvedArgTypes[i] = rType;
                this.ArgLocalIds[i].ResolvedType = rType;
            }
        }

        internal override void EnsureModifierAndTypeSignatureConsistency()
        {
            if (!(this.Owner is ClassDefinition))
            {
                this.EnsureModifierAndTypeSignatureConsistencyForNonMethods();
            }
            else
            {
                this.EnsureModifiersAndTypeSignatureConsistencyForClassMethods();
            }
        }

        private void EnsureModifierAndTypeSignatureConsistencyForNonMethods()
        {
            Token badToken = null;
            if (this.Modifiers.HasAbstract) badToken = this.Modifiers.AbstractToken;
            if (this.Modifiers.HasOverride) badToken = this.Modifiers.OverrideToken;
            if (this.Modifiers.HasStatic) badToken = this.Modifiers.StaticToken;
            if (this.Modifiers.HasPrivate) badToken = this.Modifiers.PrivateToken;
            if (this.Modifiers.HasProtected) badToken = this.Modifiers.ProtectedToken;

            if (badToken != null)
            {
                throw new ParserException(badToken, "'" + badToken.Value + "' is not a valid modifier for functions that are defined outside of a class.");
            }
        }

        private void EnsureModifiersAndTypeSignatureConsistencyForClassMethods()
        {
            ClassDefinition classDef = (ClassDefinition)this.Owner;
            ClassDefinition baseClass = classDef.BaseClass;
            bool hasBaseClass = baseClass != null;

            if (hasBaseClass)
            {
                FieldDefinition hiddenField = baseClass.GetField(this.NameToken.Value, true);
                if (hiddenField != null) throw new ParserException(this, "This function definition hides a field from a parent class.");
            }

            if (!hasBaseClass)
            {
                if (this.Modifiers.HasOverride)
                {
                    throw new ParserException(this, "Cannot mark a method as 'override' if it has no base class.");
                }
            }
            else
            {
                FunctionDefinition overriddenFunction = baseClass.GetMethod(this.NameToken.Value, true);
                if (overriddenFunction != null)
                {
                    if (!this.Modifiers.HasOverride)
                    {
                        if (this.CompilationScope.IsCrayon)
                        {
                            // TODO: just warn if not present
                        }
                        else
                        {
                            throw new ParserException(this, "This function hides another function from a parent class. If overriding is intentional, use the 'override' keyword.");
                        }
                    }

                    if (overriddenFunction.Modifiers.HasStatic)
                    {
                        throw new ParserException(this, "Cannot override a static method.");
                    }

                    if (overriddenFunction.Modifiers.AccessModifierType != this.Modifiers.AccessModifierType)
                    {
                        throw new ParserException(this, "This method defines a different access modifier than its overridden parent.");
                    }

                    if (overriddenFunction.Modifiers.AccessModifierType == AccessModifierType.PRIVATE)
                    {
                        throw new ParserException(this, "Cannot override a private method.");
                    }

                    if (overriddenFunction.Modifiers.HasInternal &&
                        overriddenFunction.CompilationScope != this.CompilationScope)
                    {
                        throw new ParserException(this, "Cannot override this method. It is marked as internal and is located in a different assembly.");
                    }

                    if (overriddenFunction.ResolvedReturnType != this.ResolvedReturnType)
                    {
                        if (overriddenFunction.ResolvedReturnType == ResolvedType.ANY &&
                            this.ResolvedReturnType == ResolvedType.OBJECT)
                        {
                            // This is fine.
                        }
                        else
                        {
                            throw new ParserException(this, "This function returns a different type than its overridden parent.");
                        }
                    }

                    if (overriddenFunction.ArgTypes.Length != this.ArgTypes.Length)
                    {
                        throw new ParserException(this, "This function has a different number of arguments than its overridden parent.");
                    }

                    if (overriddenFunction.MinArgCount != this.MinArgCount)
                    {
                        throw new ParserException(this, "This function has a different number of optional arguments than its overridden parent.");
                    }

                    // TODO: if you are overridding a function between a statically and dynamically typed languagte,
                    // the dynamic language should pick up the types from the statically typed language or the
                    // statically typed language should require "object" types in its signature for the other direction.
                    for (int i = 0; i < this.ArgTypes.Length; ++i)
                    {
                        ResolvedType expected = overriddenFunction.ResolvedArgTypes[i];
                        ResolvedType actual = this.ResolvedArgTypes[i];
                        if (actual != expected &&
                            !(actual == ResolvedType.OBJECT && expected == ResolvedType.ANY))
                        {
                            throw new ParserException(this, "This function has arguments that are different types than its overridden parent.");
                        }
                    }
                }
                else
                {
                    if (this.Modifiers.HasOverride) throw new ParserException(this, "This function is marked as 'override', but there is no method in a parent class with the same name.");
                }
            }
        }

        internal override void ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            foreach (Expression defaultValue in this.DefaultValues)
            {
                if (defaultValue != null)
                {
                    defaultValue.ResolveTypes(parser, typeResolver);
                }
            }

            foreach (Executable ex in this.Code)
            {
                ex.ResolveTypes(parser, typeResolver);
            }
        }

        internal void ResolveVariableOrigins(ParserContext parser)
        {
            VariableScope varScope = VariableScope.NewEmptyScope(this.CompilationScope.IsStaticallyTyped);
            this.ArgLocalIds = new VariableId[this.ArgNames.Length];
            for (int i = 0; i < this.ArgNames.Length; ++i)
            {
                this.ArgLocalIds[i] = varScope.RegisterVariable(this.ArgTypes[i], this.ArgNames[i].Value);
            }

            foreach (Executable ex in this.Code)
            {
                ex.ResolveVariableOrigins(parser, varScope, VariableIdAllocPhase.REGISTER_AND_ALLOC);
            }

            Lambda.DoVarScopeIdAllocationForLambdaContainer(parser, varScope, this);

            varScope.FinalizeScopeIds();

            this.LocalScopeSize = varScope.Size;

            this.Locals = varScope.GetAllLocals().ToArray();
        }
    }
}
