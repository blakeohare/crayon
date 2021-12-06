using Builder.Localization;
using Builder.ParseTree;
using Builder.Resolver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Builder
{
    internal class ParserContext
    {
        public string ProjectId { get; private set; }
        public string DelegateMainTo { get; private set; }
        public CompileRequest CompileRequest { get; private set; }

        private Stack<CompilationScope> scopeStack = new Stack<CompilationScope>();

        private Dictionary<string, CompilationScope> compilationScopes = new Dictionary<string, CompilationScope>();

        public CompilationScope RootScope { get { return this.compilationScopes["."]; } }

        public FunctionDefinition MainFunction { get; set; }
        public FunctionDefinition CoreLibInvokeFunction { get; set; }

        public int ClassIdAlloc { get; set; }
        public int ScopeIdAlloc { get; set; }

        // TODO: make this configurable.
        public bool IncludeDebugSymbols { get { return true; } }

        public TypeContext TypeContext { get; private set; }

        public ParserContext(CompileRequest compileRequest, Wax.WaxHub waxHub)
        {
            this.ClassIdAlloc = 1;
            this.ScopeIdAlloc = 1;
            this.CompileRequest = compileRequest;
            this.ProjectId = compileRequest.ProjectId;
            this.DelegateMainTo = compileRequest.DelegateMainTo;
            this.TypeContext = new TypeContext();
            Locale rootLocale = compileRequest.CompilerLocale;

            ExternalAssemblyMetadata userDefinedAssembly = CreateRootAssembly(compileRequest.CompilerLocale);
            CompilationScope userDefinedScope = new CompilationScope(this, userDefinedAssembly, rootLocale, compileRequest.RootProgrammingLanguage);

            this.PushScope(userDefinedScope);
            this.ScopeManager = new ScopeManager(compileRequest, waxHub);
            this.NamespacePrefixLookupForCurrentFile = new List<string>();
            this.ConstantAndEnumResolutionState = new Dictionary<TopLevelEntity, ConstantResolutionState>();
            this.LiteralLookup = new LiteralLookup();
        }

        private static ExternalAssemblyMetadata CreateRootAssembly(Locale locale)
        {
            ExternalAssemblyMetadata m = new ExternalAssemblyMetadata();
            m.ID = ".";
            m.InternalLocale = locale;
            m.CanonicalKey = ".";
            m.SupportedLocales = new HashSet<Locale>() { locale };
            m.OnlyImportableFrom = new HashSet<string>();
            m.IsUserDefined = true;
            return m;
        }

        private int localeCount = -1;
        public int GetLocaleCount()
        {
            if (this.localeCount == -1)
            {
                HashSet<Locale> locales = new HashSet<Locale>();
                foreach (CompilationScope scope in this.compilationScopes.Values)
                {
                    locales.Add(scope.Locale);
                }
                this.localeCount = locales.Count;
            }
            return this.localeCount;
        }

        private Dictionary<Locale, int> localeIds = null;
        public int GetLocaleId(Locale locale)
        {
            if (localeIds == null)
            {
                this.localeIds = new Dictionary<Locale, int>();
                foreach (CompilationScope scope in this.compilationScopes.Keys.OrderBy(key => key).Select(key => this.compilationScopes[key]))
                {
                    if (!this.localeIds.ContainsKey(scope.Locale))
                    {
                        this.localeIds[scope.Locale] = this.localeIds.Count;
                    }
                }
            }
            int localeId;
            if (this.localeIds.TryGetValue(locale, out localeId))
            {
                return localeId;
            }
            return -1;
        }

        public void AddCompilationScope(CompilationScope scope)
        {
            this.compilationScopes.Add(scope.ScopeKey, scope);
            this.localeCount = -1;
        }

        public Locale[] GetAllUsedLocales()
        {
            return new HashSet<Locale>(this.GetAllCompilationScopes().Select(scope => scope.Locale)).OrderBy(loc => loc.ID).ToArray();
        }

        public CompilationScope[] GetAllCompilationScopes()
        {
            return this.compilationScopes.Keys
                .OrderBy(s => s)
                .Select(key => this.compilationScopes[key])
                .ToArray();
        }

        public HashSet<string> ReservedKeywords { get; private set; }

        public void PushScope(CompilationScope scope)
        {
            this.AddCompilationScope(scope);
            this.scopeStack.Push(scope);
            this.CurrentScope = scope;
            this.CurrentLocale = scope.Locale;
            this.UpdateLanguage(this.CurrentScope);
        }

        public void PopScope()
        {
            this.scopeStack.Pop();
            this.CurrentScope = this.scopeStack.Peek();
            this.CurrentLocale = this.CurrentScope.Locale;
            this.UpdateLanguage(this.CurrentScope);
        }

        private void UpdateLanguage(CompilationScope scope)
        {
            this.Keywords = this.CurrentLocale.Keywords;
            this.ReservedKeywords = new HashSet<string>(this.CurrentLocale.GetKeywordsList());

            switch (scope.ProgrammingLanguage)
            {
                case Builder.ProgrammingLanguage.CRAYON:
                    this.TopLevelParser = new Crayon.CrayonTopLevelParser(this);
                    this.ExpressionParser = new Crayon.CrayonExpressionParser(this);
                    this.ExecutableParser = new Crayon.CrayonExecutableParser(this);
                    this.AnnotationParser = new Crayon.CrayonAnnotationParser(this);
                    this.TypeParser = new Crayon.CrayonTypeParser();
                    break;
                case Builder.ProgrammingLanguage.ACRYLIC:
                    this.TopLevelParser = new Acrylic.AcrylicTopLevelParser(this);
                    this.ExpressionParser = new Acrylic.AcrylicExpressionParser(this);
                    this.ExecutableParser = new Acrylic.AcrylicExecutableParser(this);
                    this.AnnotationParser = new Acrylic.AcrylicAnnotationParser(this);
                    this.TypeParser = new Acrylic.AcrylicTypeParser();
                    break;
            }
        }

        internal AbstractTopLevelParser TopLevelParser { get; private set; }
        internal AbstractExpressionParser ExpressionParser { get; private set; }
        internal AbstractExecutableParser ExecutableParser { get; private set; }
        internal AbstractAnnotationParser AnnotationParser { get; private set; }
        internal AbstractTypeParser TypeParser { get; private set; }

        public Locale CurrentLocale { get; private set; }
        public Locale.KeywordsLookup Keywords { get; private set; }

        public Dictionary<TopLevelEntity, ConstantResolutionState> ConstantAndEnumResolutionState { get; private set; }

        private int functionIdCounter = 0;
        private int fileIdCounter = 0;

        public int GetNextFunctionId()
        {
            return ++this.functionIdCounter;
        }

        // These did not resolve into libraries. Which might be okay as long as there's a namespace that matches
        // in the project.
        private List<ImportStatement> unresolvedImports = new List<ImportStatement>();
        private HashSet<string> allNamespaces = new HashSet<string>();

        public void RegisterNamespace(string fullNamespace)
        {
            this.allNamespaces.Add(fullNamespace);
        }

        // This is called from the resolver once all namespaces are known.
        public void VerifyNoBadImports()
        {
            foreach (ImportStatement import in unresolvedImports)
            {
                if (!allNamespaces.Contains(import.ImportPath))
                {
                    // TODO: show spelling suggestions
                    throw new ParserException(import, "Unrecognized namespace or library: '" + import.ImportPath + "'");
                }
            }
        }

        private Dictionary<ClassDefinition, int> classIdsByInstance = new Dictionary<ClassDefinition, int>();

        public TopLevelEntity CurrentCodeContainer { get; set; }

        public ExternalAssemblyMetadata CurrentLibrary { get { return this.CurrentScope.Metadata; } }

        public CompilationScope CurrentScope { get; private set; }

        public ScopeManager ScopeManager { get; private set; }

        public bool MainFunctionHasArg { get; set; }

        public List<string> NamespacePrefixLookupForCurrentFile { get; private set; }

        public HashSet<FunctionDefinition> InlinableLibraryFunctions { get; set; }

        public LiteralLookup LiteralLookup { get; private set; }

        public int GetId(string name) { return this.LiteralLookup.GetNameId(name); }
        public int GetStringConstant(string value) { return this.LiteralLookup.GetStringId(value); }
        public int GetFloatConstant(double value) { return this.LiteralLookup.GetFloatId(value); }
        public int GetBoolConstant(bool value) { return this.LiteralLookup.GetBoolId(value); }
        public int GetIntConstant(int value) { return this.LiteralLookup.GetIntId(value); }
        public int GetNullConstant() { return this.LiteralLookup.GetNullId(); }
        public int GetClassRefConstant(ClassDefinition value) { return this.LiteralLookup.GetClassRefId(value); }

        public string PopClassNameWithFirstTokenAlreadyPopped(TokenStream tokens, Token firstToken)
        {
            this.VerifyIdentifier(firstToken);
            string name = firstToken.Value;
            while (tokens.PopIfPresent("."))
            {
                Token nameNext = tokens.Pop();
                this.VerifyIdentifier(nameNext);
                name += "." + nameNext.Value;
            }
            return name;
        }

        public int GetLiteralId(Expression value)
        {
            if (value is NullConstant)
            {
                return GetNullConstant();
            }

            if (value is IntegerConstant)
            {
                return this.GetIntConstant(((IntegerConstant)value).Value);
            }

            if (value is FloatConstant)
            {
                return this.GetFloatConstant(((FloatConstant)value).Value);
            }

            if (value is BooleanConstant)
            {
                return this.GetBoolConstant(((BooleanConstant)value).Value);
            }

            if (value is StringConstant)
            {
                return this.GetStringConstant(((StringConstant)value).Value);
            }

            return -1;
        }

        // These are the lookup tables for switch statements. The ID of the switch statement is its index in this list.
        private List<Dictionary<string, int>> byteCodeSwitchStringToOffsets = new List<Dictionary<string, int>>();
        private List<Dictionary<int, int>> byteCodeSwitchIntegerToOffsets = new List<Dictionary<int, int>>();

        public List<Dictionary<int, int>> GetIntegerSwitchStatements()
        {
            return this.byteCodeSwitchIntegerToOffsets;
        }

        public List<Dictionary<string, int>> GetStringSwitchStatements()
        {
            return this.byteCodeSwitchStringToOffsets;
        }

        public int GetClassId(ClassDefinition cls)
        {
            int id;
            if (!this.classIdsByInstance.TryGetValue(cls, out id))
            {
                id = classIdsByInstance.Count + 1;
                classIdsByInstance[cls] = id;
            }
            return id;
        }

        private readonly Dictionary<int, string> filesUsed = new Dictionary<int, string>();

        private void RegisterFileUsed(FileScope file, string code)
        {
            this.filesUsed.Add(file.ID, file.Name + "\n" + code);
        }

        public string[] GetFilesById()
        {
            List<string> output = new List<string>();
            foreach (int id in this.filesUsed.Keys)
            {
                string data = this.filesUsed[id];
                while (output.Count <= id)
                {
                    output.Add(null);
                }
                output[id] = data;
            }
            return output.ToArray();
        }

        public TopLevelEntity[] ParseAllTheThings()
        {
            Dictionary<string, string> files = this.CompileRequest.GetCodeFiles();

            // When a syntax error is encountered, add it to this list (RELEASE builds only).
            // Only allow one syntax error per file. Libraries are considered stable and will
            // only report the first error in the event that an error occurs (since they get
            // parsed along with the file that imports it and so it's considered an error from
            // the importing file).
            List<ParserException> parseErrors = new List<ParserException>();

            // Only iterate through actual user files. Library imports will be inserted into the code when encountered
            // the first time for each library.
            foreach (string fileName in files.Keys)
            {
                string code = files[fileName];
#if DEBUG
                this.ParseFile(fileName, code);
#else
                try
                {
                    this.ParseFile(fileName, code);
                }
                catch (ParserException pe)
                {
                    parseErrors.Add(pe);
                }
#endif
            }

            if (parseErrors.Count > 0)
            {
                throw new MultiParserException(parseErrors);
            }

            return ResolverPipeline.Resolve(this, this.compilationScopes.Values);
        }

        public int GetNextFileId()
        {
            return fileIdCounter++;
        }

        public void ParseFile(string filename, string code)
        {
            FileScope fileScope = new FileScope(filename, code, this.CurrentScope, this.GetNextFileId());
            this.RegisterFileUsed(fileScope, code);
            TokenStream tokens = new TokenStream(fileScope);

            List<string> namespaceImportsBuilder = new List<string>();

            // Implicitly import the Core library for the current locale.
            LocalizedAssemblyView implicitCoreImport = this.ScopeManager.GetCoreLibrary(this);
            namespaceImportsBuilder.Add(implicitCoreImport.Name);
            fileScope.Imports.Add(new ImportStatement(null, implicitCoreImport.Name, fileScope));

            while (tokens.HasMore && tokens.IsNext(this.Keywords.IMPORT))
            {
                ImportStatement importStatement = this.TopLevelParser.ParseImport(tokens, fileScope);
                if (importStatement == null) throw new Exception();
                namespaceImportsBuilder.Add(importStatement.ImportPath);
                LocalizedAssemblyView localizedAssemblyView = this.ScopeManager.GetOrImportAssembly(this, importStatement.FirstToken, importStatement.ImportPath);
                if (localizedAssemblyView == null)
                {
                    this.unresolvedImports.Add(importStatement);
                }
            }

            string[] namespaceImports = namespaceImportsBuilder.ToArray();

            while (tokens.HasMore)
            {
                this.CurrentScope.AddExecutable(this.TopLevelParser.Parse(tokens, null, fileScope));
            }
        }

        internal void VerifyIdentifier(Token token)
        {
            if (token.Type == TokenType.KEYWORD)
            {
                throw new ParserException(token, "'" + token.Value + "' cannot be used as a name because it is a keyword.");
            }
            else if (token.Type != TokenType.WORD)
            {
                throw new ParserException(token, "Identifier expected. Found '" + token.Value + "' instead.");
            }
        }

        public Executable[] Resolve(IList<Executable> rawParsedLines)
        {
            List<Executable> output = new List<Executable>();
            foreach (Executable line in rawParsedLines)
            {
                output.AddRange(line.Resolve(this));
            }
            return output.ToArray();
        }

        public int ValueStackDepth { get; set; }

        public Exception GenerateParseError(ErrorMessages errorType, Token token, params string[] args)
        {
            return new ParserException(token, this.CurrentLocale.Strings.Get(errorType.ToString(), args));
        }
    }
}
