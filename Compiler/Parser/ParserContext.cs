using Build;
using Common;
using Localization;
using Parser.Crayon;
using Parser.ParseTree;
using Parser.Resolver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Parser
{
    public class ParserContext
    {
        private Stack<CompilationScope> scopeStack = new Stack<CompilationScope>();

        private Dictionary<string, CompilationScope> compilationScopes = new Dictionary<string, CompilationScope>();

        public CompilationScope RootScope { get { return this.compilationScopes["."]; } }

        public FunctionDefinition MainFunction { get; set; }
        public FunctionDefinition CoreLibInvokeFunction { get; set; }

        public ParserContext(BuildContext buildContext)
        {
            this.BuildContext = buildContext;
            this.PushScope(new CompilationScope(buildContext, new AssemblyMetadata(buildContext.CompilerLocale)));
            this.AssemblyManager = new AssemblyManager(buildContext);
            this.NamespacePrefixLookupForCurrentFile = new List<string>();
            this.ConstantAndEnumResolutionState = new Dictionary<TopLevelEntity, ConstantResolutionState>();
            this.LiteralLookup = new LiteralLookup();

            this.TopLevelParser = new TopLevelParser(this);
            this.ExpressionParser = new ExpressionParser(this);
            this.ExecutableParser = new ExecutableParser(this);
            this.AnnotationParser = new AnnotationParser(this);
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
            this.Keywords = this.CurrentLocale.Keywords;
            this.ReservedKeywords = new HashSet<string>(this.CurrentLocale.GetKeywordsList());
        }

        public void PopScope()
        {
            this.scopeStack.Pop();
            this.CurrentScope = this.scopeStack.Peek();
            this.CurrentLocale = this.CurrentScope.Locale;
            this.Keywords = this.CurrentLocale.Keywords;
            this.ReservedKeywords = new HashSet<string>(this.CurrentLocale.GetKeywordsList());
        }

        internal ITopLevelParser TopLevelParser { get; private set; }
        internal IExpressionParser ExpressionParser { get; private set; }
        internal IExecutableParser ExecutableParser { get; private set; }
        internal IAnnotationParser AnnotationParser { get; private set; }

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

        public AssemblyMetadata CurrentLibrary { get { return this.CurrentScope.Metadata; } }

        public CompilationScope CurrentScope { get; private set; }

        public AssemblyManager AssemblyManager { get; private set; }

        public bool MainFunctionHasArg { get; set; }

        public BuildContext BuildContext { get; private set; }

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

        private Dictionary<string, Dictionary<string, int>> stringSwitchLookups = new Dictionary<string, Dictionary<string, int>>();
        private Dictionary<string, Dictionary<int, int>> intListLookups = new Dictionary<string, Dictionary<int, int>>();
        private Dictionary<string, int> explicitMaxes = new Dictionary<string, int>();
        private Dictionary<string, int> defaultCaseIds = new Dictionary<string, int>();

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

        private Dictionary<string, ClassDefinition> classDefinitions = new Dictionary<string, ClassDefinition>();

        public ClassDefinition GetClass(string name)
        {
            if (this.classDefinitions.ContainsKey(name))
            {
                return this.classDefinitions[name];
            }
            return null;
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
            Dictionary<string, string> files = this.BuildContext.TopLevelAssembly.GetCodeFiles();

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
            LocalizedAssemblyView implicitCoreImport = this.AssemblyManager.GetCoreLibrary(this);
            namespaceImportsBuilder.Add(implicitCoreImport.Name);
            fileScope.Imports.Add(new ImportStatement(null, implicitCoreImport.Name, fileScope));

            while (tokens.HasMore && tokens.IsNext(this.Keywords.IMPORT))
            {
                ImportStatement importStatement = this.TopLevelParser.ParseImport(tokens, fileScope);
                if (importStatement == null) throw new Exception();
                namespaceImportsBuilder.Add(importStatement.ImportPath);
                LocalizedAssemblyView localizedAssemblyView = this.AssemblyManager.GetOrImportAssembly(this, importStatement.FirstToken, importStatement.ImportPath);
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
            if (token.Type != TokenType.WORD)
            {
                throw new ParserException(token, "Identifier expected. Found '" + token.Value + "' instead.");
            }
        }

        private readonly HashSet<string> RESERVED_KEYWORDS = new HashSet<string>();

        public Executable[] Resolve(IList<Executable> rawParsedLines)
        {
            List<Executable> output = new List<Executable>();
            foreach (Executable line in rawParsedLines)
            {
                output.AddRange(line.Resolve(this));
            }
            return output.ToArray();
        }

        public string GetSwitchLookupCode()
        {
            List<string> output = new List<string>();
            foreach (string key in this.stringSwitchLookups.Keys)
            {
                string lookupName = key;
                Dictionary<string, int> valuesToInts = this.stringSwitchLookups[key];
                output.Add(lookupName);
                output.Add(" = { ");
                bool first = true;
                foreach (string skey in valuesToInts.Keys)
                {
                    if (!first)
                    {
                        first = false;
                        output.Add(", ");
                    }
                    output.Add(Util.ConvertStringValueToCode(skey));
                    output.Add(": ");
                    output.Add("" + valuesToInts[skey]);
                }
                output.Add(" };\r\n");
            }

            foreach (string lookupName in this.intListLookups.Keys)
            {
                List<int> actualList = new List<int>();
                Dictionary<int, int> lookup = this.intListLookups[lookupName];
                int explicitMax = this.explicitMaxes[lookupName];
                int defaultCaseId = this.defaultCaseIds[lookupName];
                while (actualList.Count <= explicitMax)
                {
                    actualList.Add(defaultCaseId);
                }

                foreach (int ikey in lookup.Keys)
                {
                    while (actualList.Count <= ikey)
                    {
                        actualList.Add(defaultCaseId);
                    }
                    actualList[ikey] = lookup[ikey];
                }

                output.Add(lookupName);
                output.Add(" = [");
                for (int i = 0; i < actualList.Count; ++i)
                {
                    if (i > 0) output.Add(", ");
                    output.Add(actualList[i] + "");
                }
                output.Add("];\r\n");
            }

            return string.Join("", output);
        }

        public int ValueStackDepth { get; set; }

        public Exception GenerateParseError(ErrorMessages errorType, Token token, params string[] args)
        {
            return new ParserException(token, this.CurrentLocale.Strings.Get(errorType.ToString(), args));
        }
    }
}
