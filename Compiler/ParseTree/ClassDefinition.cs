using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
    internal class ClassDefinition : TopLevelConstruct
    {
        private static int classIdAlloc = 1;

        public int ClassID { get; private set; }
        public ClassDefinition BaseClass { get; private set; }
        public Token NameToken { get; private set; }
        public Token[] BaseClassTokens { get; private set; }
        public string[] BaseClassDeclarations { get; private set; }
        // TODO: interface definitions.
        public FunctionDefinition[] Methods { get; set; }
        public ConstructorDefinition Constructor { get; set; }
        public ConstructorDefinition StaticConstructor { get; set; }
        public FieldDeclaration[] Fields { get; set; }

        public Token StaticToken { get; set; }
        public Token FinalToken { get; set; }

        private bool memberIdsResolved = false;

        // When a variable in this class is not locally defined, look for a fully qualified name that has one of these prefixes.

        private Dictionary<string, FunctionDefinition> functionDefinitionsByName = null;
        private Dictionary<string, FieldDeclaration> fieldDeclarationsByName = null;

        public ClassDefinition(
            Token classToken,
            Token nameToken,
            IList<Token> subclassTokens,
            IList<string> subclassNames,
            string ns,
            TopLevelConstruct owner,
            Library library,
            Token staticToken,
            Token finalToken,
            FileScope fileScope)
            : base(classToken, owner, fileScope)
        {
            this.Library = library;
            this.ClassID = ClassDefinition.classIdAlloc++;

            this.Namespace = ns;
            this.NameToken = nameToken;
            this.BaseClassTokens = subclassTokens.ToArray();
            this.BaseClassDeclarations = subclassNames.ToArray();
            this.StaticToken = staticToken;
            this.FinalToken = finalToken;

            if (staticToken != null && this.BaseClassTokens.Length > 0)
            {
                throw new ParserException(staticToken, "Class cannot be static and have base classes or interfaces.");
            }
        }

        private static readonly VariableIdAllocator EMPTY_VAR_ALLOC = new VariableIdAllocator();
        public void AllocateLocalScopeIds(Parser parser)
        {
            foreach (FieldDeclaration fd in this.Fields)
            {
                fd.PerformLocalIdAllocation(parser, EMPTY_VAR_ALLOC, VariableIdAllocPhase.ALLOC);
            }

            // null check has occurred before now.
            this.Constructor.AllocateLocalScopeIds(parser);

            if (this.StaticConstructor != null)
            {
                this.StaticConstructor.AllocateLocalScopeIds(parser);
            }

            foreach (FunctionDefinition fd in this.Methods)
            {
                fd.AllocateLocalScopeIds(parser);
            }
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

        public FieldDeclaration GetField(string name, bool walkUpBaseClasses)
        {
            if (this.fieldDeclarationsByName == null)
            {
                this.fieldDeclarationsByName = new Dictionary<string, FieldDeclaration>();

                int staticMemberId = 0;

                foreach (FieldDeclaration fd in this.Fields)
                {
                    this.fieldDeclarationsByName[fd.NameToken.Value] = fd;
                    if (fd.IsStaticField)
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

        internal override void Resolve(Parser parser)
        {
            if (parser.IsInClass)
            {
                throw new ParserException(this.FirstToken, "Nested classes aren't a thing, yet.");
            }

            if (parser.IsReservedKeyword(this.NameToken.Value))
            {
                throw new ParserException(this.NameToken, "'" + this.NameToken.Value + "' is a reserved keyword.");
            }

            parser.CurrentClass = this;

            for (int i = 0; i < this.Fields.Length; ++i)
            {

                FieldDeclaration field = this.Fields[i];
                field.Resolve(parser);
                this.Fields[i] = field;
                if (this.StaticToken != null && !field.IsStaticField)
                {
                    throw new ParserException(field.FirstToken, "Cannot have a non-static field in a static class.");
                }
            }

            for (int i = 0; i < this.Methods.Length; ++i)
            {
                FunctionDefinition funcDef = this.Methods[i];
                funcDef.Resolve(parser);
                this.Methods[i] = funcDef;
                if (this.StaticToken != null && !funcDef.IsStaticMethod)
                {
                    throw new ParserException(funcDef.FirstToken, "Cannot have a non-static method in a static class.");
                }
            }

            this.Constructor.Resolve(parser);
            if (this.StaticToken != null && !this.Constructor.IsDefault)
            {
                throw new ParserException(this.Constructor.FirstToken, "Static classes cannot have a non-static constructor.");
            }

            if (this.StaticConstructor != null)
            {
                this.StaticConstructor.Resolve(parser);
            }

            parser.CurrentClass = null;

            bool hasABaseClass = this.BaseClass != null;
            bool callsBaseConstructor = this.Constructor.BaseToken != null;

            if (hasABaseClass)
            {
                if (this.BaseClass.FinalToken != null)
                {
                    throw new ParserException(this.FirstToken, "This class extends from " + this.BaseClass.NameToken.Value + " which is marked as final.");
                }

                if (this.BaseClass.StaticToken != null)
                {
                    throw new ParserException(this.FirstToken, "This class extends from " + this.BaseClass.NameToken.Value + " which is marked as static.");
                }
            }

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
                    throw new ParserException(this.FirstToken, "The base class of this class has a constructor which must be called.");
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
                TopLevelConstruct baseClassInstance = this.FileScope.FileScopeEntityLookup.DoLookup(value, this);
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

        internal override void ResolveNames(Parser parser)
        {
            foreach (FieldDeclaration fd in this.Fields)
            {
                fd.ResolveNames(parser);
            }

            if (this.StaticConstructor != null)
            {
                this.StaticConstructor.ResolveNames(parser);
            }

            // This should be empty if there is no base class, or just pass along the base class' args if there is.
            if (this.Constructor == null)
            {
                this.Constructor = new ConstructorDefinition(this);
            }

            this.Constructor.ResolveNames(parser);
            this.BatchTopLevelConstructNameResolver(parser, this.Methods);
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
                    throw new ParserException(this.FirstToken, "This class' parent class hierarchy creates a cycle.");
                }
                classIds.Add(walker.ClassID);
                walker = walker.BaseClass;
            }
        }

        private Dictionary<string, TopLevelConstruct> flattenedFieldAndMethodDeclarationsByName = new Dictionary<string, TopLevelConstruct>();

        public void ResolveMemberIds()
        {
            if (this.memberIdsResolved) return;

            if (this.BaseClass != null)
            {
                this.BaseClass.ResolveMemberIds();
                Dictionary<string, TopLevelConstruct> parentDefinitions = this.BaseClass.flattenedFieldAndMethodDeclarationsByName;
                foreach (string key in parentDefinitions.Keys)
                {
                    this.flattenedFieldAndMethodDeclarationsByName[key] = parentDefinitions[key];
                }
            }

            foreach (FieldDeclaration fd in this.Fields)
            {
                TopLevelConstruct existingItem;
                if (this.flattenedFieldAndMethodDeclarationsByName.TryGetValue(fd.NameToken.Value, out existingItem))
                {
                    // TODO: definition of a field or a method? from this class or a parent?
                    // TODO: check to see if this is already resolved before now.
                    throw new ParserException(fd.FirstToken, "This overrides a previous definition of a member named '" + fd.NameToken.Value + "'.");
                }

                fd.MemberID = this.flattenedFieldAndMethodDeclarationsByName.Count;
                this.flattenedFieldAndMethodDeclarationsByName[fd.NameToken.Value] = fd;
            }

            foreach (FunctionDefinition fd in this.Methods)
            {
                if (!fd.IsStaticMethod)
                {
                    TopLevelConstruct existingItem;
                    if (this.flattenedFieldAndMethodDeclarationsByName.TryGetValue(fd.NameToken.Value, out existingItem))
                    {
                        if (existingItem is FieldDeclaration)
                        {
                            // TODO: again, give more information.
                            throw new ParserException(fd.FirstToken, "This field overrides a previous definition.");
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

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }
        internal override void PerformLocalIdAllocation(Parser parser, VariableIdAllocator varIds, VariableIdAllocPhase phase)
        {
            // Not called in this way.
            throw new System.NotImplementedException();
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
    }
}
