using Parser.Localization;
using Parser.Resolver;
using System.Collections.Generic;
using System.Linq;

namespace Parser.ParseTree
{
    internal class ClassDefinition : TopLevelEntity
    {
        public int ClassID { get; private set; }
        public ClassDefinition BaseClass { get; private set; }
        public Token NameToken { get; private set; }
        public Token[] BaseClassTokens { get; private set; }
        public string[] BaseClassDeclarations { get; private set; }
        // TODO: interface definitions.
        public FunctionDefinition[] Methods { get; set; }
        public ConstructorDefinition Constructor { get; set; }
        public ConstructorDefinition StaticConstructor { get; set; }
        public FieldDefinition[] Fields { get; set; }

        private bool memberIdsResolved = false;
        private AnnotationCollection annotations;

        // When a variable in this class is not locally defined, look for a fully qualified name that has one of these prefixes.

        private Dictionary<string, FunctionDefinition> functionDefinitionsByName = null;
        private Dictionary<string, FieldDefinition> fieldDeclarationsByName = null;

        public ClassDefinition(
            Token classToken,
            Token nameToken,
            IList<Token> subclassTokens,
            IList<string> subclassNames,
            Node owner,
            FileScope fileScope,
            ModifierCollection modifiers,
            AnnotationCollection annotations,
            ParserContext context)
            : base(classToken, owner, fileScope, modifiers)
        {
            this.ClassID = context.ClassIdAlloc++;

            this.NameToken = nameToken;
            this.BaseClassTokens = subclassTokens.ToArray();
            this.BaseClassDeclarations = subclassNames.ToArray();
            this.annotations = annotations;

            if (this.Modifiers.HasPrivate) throw new ParserException(this.Modifiers.PrivateToken, "Private classes are not supported yet.");
            if (this.Modifiers.HasProtected) throw new ParserException(this.Modifiers.ProtectedToken, "Protected classes are not supported yet.");
        }

        private Dictionary<Locale, string> namesByLocale = null;
        public override string GetFullyQualifiedLocalizedName(Locale locale)
        {
            if (this.namesByLocale == null) this.namesByLocale = this.annotations.GetNamesByLocale(1);
            string name = this.NameToken.Value;
            if (this.namesByLocale.ContainsKey(locale)) name = this.namesByLocale[locale];

            if (this.Owner != null)
            {
                name = this.TopLevelEntity.GetFullyQualifiedLocalizedName(locale) + "." + name;
            }
            return name;
        }

        internal void ResolveVariableOrigins(ParserContext parser)
        {
            foreach (FieldDefinition fd in this.Fields)
            {
                fd.ResolveVariableOrigins(parser);
            }

            // null check has occurred before now.
            this.Constructor.ResolveVariableOrigins(parser);

            if (this.StaticConstructor != null)
            {
                this.StaticConstructor.ResolveVariableOrigins(parser);
            }

            foreach (FunctionDefinition fd in this.Methods)
            {
                fd.ResolveVariableOrigins(parser);
            }
        }

        public TopLevelEntity GetMember(string name, bool walkUpBaseClasses)
        {
            return this.GetField(name, walkUpBaseClasses) ?? (TopLevelEntity)this.GetMethod(name, walkUpBaseClasses);
        }

        public FunctionDefinition GetMethod(string name, bool walkUpBaseClasses)
        {
            if (this.functionDefinitionsByName == null)
            {
                this.functionDefinitionsByName = new Dictionary<string, FunctionDefinition>();

                foreach (FunctionDefinition fd in this.Methods)
                {
                    this.functionDefinitionsByName[fd.NameToken.Value] = fd;
                }
            }

            if (this.functionDefinitionsByName.ContainsKey(name))
            {
                return this.functionDefinitionsByName[name];
            }

            if (walkUpBaseClasses && this.BaseClass != null)
            {
                return this.BaseClass.GetMethod(name, walkUpBaseClasses);
            }

            return null;
        }

        public FieldDefinition GetField(string name, bool walkUpBaseClasses)
        {
            if (this.fieldDeclarationsByName == null)
            {
                this.fieldDeclarationsByName = new Dictionary<string, FieldDefinition>();

                int staticMemberId = 0;

                foreach (FieldDefinition fd in this.Fields)
                {
                    this.fieldDeclarationsByName[fd.NameToken.Value] = fd;
                    if (fd.Modifiers.HasStatic)
                    {
                        fd.StaticMemberID = staticMemberId++;
                    }
                }
            }

            if (this.fieldDeclarationsByName.ContainsKey(name))
            {
                return this.fieldDeclarationsByName[name];
            }

            if (walkUpBaseClasses && this.BaseClass != null)
            {
                return this.BaseClass.GetField(name, walkUpBaseClasses);
            }

            return null;
        }

        internal override void Resolve(ParserContext parser)
        {
            for (int i = 0; i < this.Fields.Length; ++i)
            {
                FieldDefinition field = this.Fields[i];
                field.Resolve(parser);
                this.Fields[i] = field;
            }

            for (int i = 0; i < this.Methods.Length; ++i)
            {
                FunctionDefinition funcDef = this.Methods[i];
                funcDef.Resolve(parser);
                this.Methods[i] = funcDef;
            }

            this.Constructor.Resolve(parser);

            if (this.StaticConstructor != null)
            {
                this.StaticConstructor.Resolve(parser);
            }

            bool hasABaseClass = this.BaseClass != null;
            bool callsBaseConstructor = this.Constructor.BaseToken != null;

            if (hasABaseClass && callsBaseConstructor)
            {
                Expression[] defaultValues = this.BaseClass.Constructor.DefaultValues;
                int maxValues = defaultValues.Length;
                int minValues = 0;
                for (int i = 0; i < maxValues; ++i)
                {
                    if (defaultValues[i] == null) minValues++;
                    else break;
                }
                int baseArgs = this.Constructor.BaseArgs.Length;
                if (baseArgs < minValues || baseArgs > maxValues)
                {
                    throw new ParserException(this.Constructor.BaseToken, "Invalid number of arguments passed to base constructor.");
                }
            }
            else if (hasABaseClass && !callsBaseConstructor)
            {
                if (this.BaseClass.Constructor != null)
                {
                    if (this.BaseClass.Constructor.MinArgCount > 0)
                    {
                        throw new ParserException(this, "The base class of this class has a constructor which must be called.");
                    }
                }
            }
            else if (!hasABaseClass && callsBaseConstructor)
            {
                throw new ParserException(this.Constructor.BaseToken, "Cannot call base constructor without a base class.");
            }
            else // (!hasABaseClass && !callsBaseConstructor)
            {
                // yeah, that's fine.
            }
        }

        public void ResolveBaseClasses()
        {
            List<ClassDefinition> baseClasses = new List<ClassDefinition>();
            List<Token> baseClassesTokens = new List<Token>();
            for (int i = 0; i < this.BaseClassDeclarations.Length; ++i)
            {
                string value = this.BaseClassDeclarations[i];
                Token token = this.BaseClassTokens[i];
                TopLevelEntity baseClassInstance = this.FileScope.FileScopeEntityLookup.DoEntityLookup(value, this);
                if (baseClassInstance == null)
                {
                    throw new ParserException(token, "No class named '" + token.Value + "' was found.");
                }

                if (baseClassInstance is ClassDefinition)
                {
                    baseClasses.Add((ClassDefinition)baseClassInstance);
                    baseClassesTokens.Add(token);
                }
                // TODO: else if (baseClassInstance is InterfaceDefinition) { ... }
                else
                {
                    throw new ParserException(token, "This is not a class.");
                }
            }

            if (baseClasses.Count > 1)
            {
                throw new ParserException(baseClassesTokens[1], "Multiple base classes found. Did you mean to use an interface?");
            }

            if (baseClasses.Count == 1)
            {
                this.BaseClass = baseClasses[0];
            }
        }

        internal override void ResolveEntityNames(ParserContext parser)
        {
            foreach (FieldDefinition fd in this.Fields)
            {
                fd.ResolveEntityNames(parser);
            }

            if (this.StaticConstructor != null)
            {
                this.StaticConstructor.ResolveEntityNames(parser);
            }

            this.Constructor.ResolveEntityNames(parser);
            this.BatchTopLevelConstructNameResolver(parser, this.Methods);
        }

        private IEnumerable<TopLevelEntity> GetAllMembersAsEnumerable()
        {
            return new TopLevelEntity[] { this.Constructor, this.StaticConstructor }
                .Where(tle => tle != null)
                .Concat(this.Methods)
                .Concat(this.Fields)
                .OrderBy(tle =>
                    // iteration order is declaration order
                    tle.FirstToken == null
                        ? 0
                        : tle.FirstToken.Line * 10000000.0 + tle.FirstToken.Col);
        }

        internal override void ResolveSignatureTypes(ParserContext parser, TypeResolver typeResolver)
        {
            foreach (TopLevelEntity tle in this.GetAllMembersAsEnumerable())
            {
                TypeResolver memberTypeResolver = new TypeResolver(parser.TypeContext, tle);
                tle.ResolveSignatureTypes(parser, memberTypeResolver);
            }
        }

        internal override void EnsureModifierAndTypeSignatureConsistency(TypeContext tc)
        {
            bool isStatic = this.Modifiers.HasStatic;
            if (this.BaseClass != null)
            {
                if (isStatic) throw new ParserException(this.Modifiers.StaticToken, "Cannot mark a class static and inhereit from another class.");
                if (this.BaseClass.Modifiers.HasFinal) throw new ParserException(this, "Cannot inherit from a final class");
                if (this.BaseClass.Modifiers.HasPrivate) throw new ParserException(this, "Cannot inherit from a private class");
                if (this.BaseClass.Modifiers.AccessModifierType != AccessModifierType.PUBLIC)
                {
                    if (this.BaseClass.CompilationScope != this.CompilationScope)
                    {
                        throw new ParserException(this, "The base class is in a different compilation scope and is not public.");
                    }
                }
            }

            foreach (FieldDefinition field in this.Fields)
            {
                field.EnsureModifierAndTypeSignatureConsistency(tc);
            }

            foreach (FunctionDefinition func in this.Methods)
            {
                func.EnsureModifierAndTypeSignatureConsistency(tc);
            }

            this.Constructor.EnsureModifierAndTypeSignatureConsistency(tc);
            if (this.StaticConstructor != null)
            {
                this.StaticConstructor.EnsureModifierAndTypeSignatureConsistency(tc);
            }
        }

        internal override void ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            foreach (TopLevelEntity tle in this.GetAllMembersAsEnumerable())
            {
                TypeResolver memberTypeResolver = new TypeResolver(parser.TypeContext, tle);
                tle.ResolveTypes(parser, memberTypeResolver);
            }
        }

        public void VerifyNoBaseClassLoops()
        {
            if (this.BaseClass == null) return;

            HashSet<int> classIds = new HashSet<int>();
            ClassDefinition walker = this;
            while (walker != null)
            {
                if (classIds.Contains(walker.ClassID))
                {
                    throw new ParserException(this, "This class' parent class hierarchy creates a cycle.");
                }
                classIds.Add(walker.ClassID);
                walker = walker.BaseClass;
            }
        }

        private Dictionary<string, TopLevelEntity> flattenedFieldAndMethodDeclarationsByName = new Dictionary<string, TopLevelEntity>();

        public void ResolveMemberIds()
        {
            if (this.memberIdsResolved) return;

            if (this.BaseClass != null)
            {
                this.BaseClass.ResolveMemberIds();
                Dictionary<string, TopLevelEntity> parentDefinitions = this.BaseClass.flattenedFieldAndMethodDeclarationsByName;
                foreach (string key in parentDefinitions.Keys)
                {
                    this.flattenedFieldAndMethodDeclarationsByName[key] = parentDefinitions[key];
                }
            }

            foreach (FieldDefinition fd in this.Fields)
            {
                TopLevelEntity existingItem;
                if (this.flattenedFieldAndMethodDeclarationsByName.TryGetValue(fd.NameToken.Value, out existingItem))
                {
                    // TODO: definition of a field or a method? from this class or a parent?
                    // TODO: check to see if this is already resolved before now.
                    throw new ParserException(fd, "This overrides a previous definition of a member named '" + fd.NameToken.Value + "'.");
                }

                fd.MemberID = this.flattenedFieldAndMethodDeclarationsByName.Count;
                this.flattenedFieldAndMethodDeclarationsByName[fd.NameToken.Value] = fd;
            }

            foreach (FunctionDefinition fd in this.Methods)
            {
                if (!fd.Modifiers.HasStatic)
                {
                    TopLevelEntity existingItem;
                    if (this.flattenedFieldAndMethodDeclarationsByName.TryGetValue(fd.NameToken.Value, out existingItem))
                    {
                        if (existingItem is FieldDefinition)
                        {
                            // TODO: again, give more information.
                            throw new ParserException(fd, "This field overrides a previous definition.");
                        }

                        // Take the old member ID it overrides.
                        fd.MemberID = ((FunctionDefinition)existingItem).MemberID;
                    }
                    else
                    {
                        fd.MemberID = this.flattenedFieldAndMethodDeclarationsByName.Count;
                    }

                    this.flattenedFieldAndMethodDeclarationsByName[fd.NameToken.Value] = fd;
                }
            }

            this.memberIdsResolved = true;
        }

        public bool ExtendsFrom(ClassDefinition cd)
        {
            ClassDefinition walker = this;
            while (walker != null)
            {
                if (walker == cd) return true;
                walker = walker.BaseClass;
            }
            return false;
        }

        internal ClassDefinition GetCommonAncestor(ClassDefinition other)
        {
            if (other == this) return this;
            if (other.BaseClass == this) return this;
            if (other == this.BaseClass) return other;
            ClassDefinition[] thisChain = this.GetClassAncestryChain();
            if (thisChain[0] == other) return other;
            ClassDefinition[] otherChain = other.GetClassAncestryChain();
            if (thisChain[0] != otherChain[0]) return null;
            ClassDefinition commonAncestor = thisChain[0];
            int length = System.Math.Min(thisChain.Length, otherChain.Length);
            for (int i = 1; i < length; ++i)
            {
                if (thisChain[i] == otherChain[i]) commonAncestor = thisChain[i];
                else break;
            }
            return commonAncestor;
        }

        private ClassDefinition[] ancestryCache = null;
        private ClassDefinition[] GetClassAncestryChain()
        {
            if (ancestryCache == null && this.BaseClass != null)
            {
                ClassDefinition walker = this;
                List<ClassDefinition> output = new List<ClassDefinition>();
                while (walker != null)
                {
                    output.Add(walker);
                    walker = walker.BaseClass;
                }
                output.Reverse();
                ancestryCache = output.ToArray();
            }
            return ancestryCache;
        }
    }
}
