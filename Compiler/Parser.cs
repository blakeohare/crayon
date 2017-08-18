﻿using System;
using System.Collections.Generic;
using Crayon.ParseTree;
using Common;

namespace Crayon
{
    internal class Parser
    {
        public Parser(BuildContext buildContext, LibraryManager sysLibMan)
        {
            this.CurrentClass = null;
            this.CurrentSystemLibrary = null;
            this.BuildContext = buildContext;
            this.LibraryManager = sysLibMan ?? new LibraryManager(null);
            this.CurrentNamespace = "";
            this.NamespacePrefixLookupForCurrentFile = new List<string>();
            this.ConstantAndEnumResolutionState = new Dictionary<Executable, ConstantResolutionState>();
            this.Locale = buildContext.CompilerLocale;
            this.Keywords = this.Locale.Keywords;
            this.RESERVED_KEYWORDS.UnionWith(this.Locale.GetKeywordsList());
            this.ExpressionParser = new ExpressionParser(this);
            this.ExecutableParser = new ExecutableParser(this);
            this.AnnotationParser = new AnnotationParser(this);
            this.localeStack = new Stack<Locale>();
            this.localeStack.Push(this.Locale);
        }

        private Stack<Locale> localeStack;

        public void PushLocale(Locale locale)
        {
            this.localeStack.Push(locale);
            this.Locale = locale;
            this.Keywords = this.Locale.Keywords;
        }

        public void PopLocale()
        {
            this.localeStack.Pop();
            this.Locale = this.localeStack.Peek();
            this.Keywords = this.Locale.Keywords;
        }

        private Token[] GetImplicitCoreImport()
        {
            return Tokenizer.Tokenize("implicit code", this.Keywords.IMPORT + " Core;", -1, true);
        }

        public ExpressionParser ExpressionParser { get; private set; }
        public ExecutableParser ExecutableParser { get; private set; }
        public AnnotationParser AnnotationParser { get; private set; }

        public Locale Locale { get; private set; }
        public Locale.KeywordsLookup Keywords { get; private set; }

        public Dictionary<Executable, ConstantResolutionState> ConstantAndEnumResolutionState { get; private set; }

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
                    throw new ParserException(import.FirstToken, "Unrecognized namespace or library: '" + import.ImportPath + "'");
                }
            }
        }

        private Dictionary<ClassDefinition, int> classIdsByInstance = new Dictionary<ClassDefinition, int>();

        // HACK ALERT - Forgive me father for I have sinned.
        // I need an access-anywhere boolean flag to determine if the parser is running in translate mode.
        // Syntax parsing is currently stateless. Which is nice. In an ideal world.
        // One day I will undo this for a more reasonable solution.
        public static bool IsTranslateMode_STATIC_HACK { get; set; }

        public Executable CurrentCodeContainer { get; set; }

        public static string CurrentSystemLibrary_STATIC_HACK { get; set; }
        public string CurrentSystemLibrary { get; set; }

        public LibraryManager LibraryManager { get; private set; }

        public ClassDefinition CurrentClass { get; set; }

        public bool MainFunctionHasArg { get; set; }

        public bool IsInClass { get { return this.CurrentClass != null; } }

        public BuildContext BuildContext { get; private set; }

        public List<string> NamespacePrefixLookupForCurrentFile { get; private set; }

        public HashSet<FunctionDefinition> InlinableLibraryFunctions { get; set; }

        public LiteralLookup LiteralLookup { get { return this.literalLookup; } }
        private LiteralLookup literalLookup = new LiteralLookup();
        public int GetId(string name) { return this.literalLookup.GetNameId(name); }
        public int GetStringConstant(string value) { return this.literalLookup.GetStringId(value); }
        public int GetFloatConstant(double value) { return this.literalLookup.GetFloatId(value); }
        public int GetBoolConstant(bool value) { return this.literalLookup.GetBoolId(value); }
        public int GetIntConstant(int value) { return this.literalLookup.GetIntId(value); }
        public int GetNullConstant() { return this.literalLookup.GetNullId(); }
        public int GetClassRefConstant(ClassDefinition value) { return this.literalLookup.GetClassRefId(value); }

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

        public int RegisterByteCodeSwitch(Dictionary<int, int> chunkIdsToOffsets, Dictionary<int, int> integersToChunkIds, Dictionary<string, int> stringsToChunkIds, bool isIntegerSwitch)
        {
            int switchId;
            if (isIntegerSwitch)
            {
                switchId = byteCodeSwitchIntegerToOffsets.Count;
                Dictionary<int, int> integersToOffsets = new Dictionary<int, int>();
                foreach (int key in integersToChunkIds.Keys)
                {
                    int chunkId = integersToChunkIds[key];
                    integersToOffsets[key] = chunkIdsToOffsets[chunkId];
                }
                byteCodeSwitchIntegerToOffsets.Add(integersToOffsets);
            }
            else
            {
                switchId = byteCodeSwitchStringToOffsets.Count;
                Dictionary<string, int> stringsToOffsets = new Dictionary<string, int>();
                foreach (string key in stringsToChunkIds.Keys)
                {
                    int chunkId = stringsToChunkIds[key];
                    stringsToOffsets[key] = chunkIdsToOffsets[chunkId];
                }
                byteCodeSwitchStringToOffsets.Add(stringsToOffsets);
            }
            return switchId;
        }

        public void RegisterSwitchIntegerListLookup(string name, Dictionary<int, int> lookup, int explicitMax, int defaultCaseId)
        {
            this.explicitMaxes[name] = explicitMax;
            this.defaultCaseIds[name] = defaultCaseId;
            this.intListLookups[name] = lookup;
        }

        public void RegisterSwitchStringDictLookup(string name, Dictionary<string, int> lookup)
        {
            this.stringSwitchLookups[name] = lookup;
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

        private Dictionary<int, string> filesUsed = new Dictionary<int, string>();

        private void RegisterFileUsed(string filename, string code, int fileId)
        {
            this.filesUsed.Add(fileId, filename + "\n" + code);
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

        public Executable[] ParseInterpreterCode(string filename, string contents)
        {
            TokenStream tokens = new TokenStream(Tokenizer.Tokenize(filename, contents, 0, true), filename);
            List<Executable> output = new List<Executable>();
            while (tokens.HasMore)
            {
                output.Add(this.ExecutableParser.Parse(tokens, false, true, true, null));
            }
            return new Resolver(this, output).ResolveTranslatedCode();
        }

        public Dictionary<string, string> GetCodeFiles()
        {
            Dictionary<string, string> output = new Dictionary<string, string>();

            foreach (FilePath sourceDir in this.BuildContext.SourceFolders)
            {
                string[] files = FileUtil.GetAllAbsoluteFilePathsDescendentsOf(sourceDir.AbsolutePath);
                foreach (string filepath in files)
                {
                    if (filepath.ToLowerInvariant().EndsWith(".cry"))
                    {
                        string relativePath = FileUtil.ConvertAbsolutePathToRelativePath(
                            filepath,
                            this.BuildContext.ProjectDirectory);
                        output[relativePath] = FileUtil.ReadFileText(filepath);
                    }
                }
            }
            return output;
        }

        public Executable[] ParseAllTheThings()
        {
            List<Executable> output = new List<Executable>();
            Dictionary<string, string> files = this.GetCodeFiles();
            // Only iterate through actual user files. Library imports will be inserted into the code when encountered
            // the first time for each library.
            foreach (string fileName in files.Keys)
            {
                string code = files[fileName];
                Executable[] fileContent = this.ParseInterpretedCode(fileName, code, null);
                output.AddRange(fileContent);
            }
            return new Resolver(this, output).ResolveInterpretedCode();
        }

        public int GetNextFileId()
        {
            return fileIdCounter++;
        }

        public Executable[] ParseInterpretedCode(string filename, string code, string libraryName)
        {
            int fileId = this.GetNextFileId();
            this.RegisterFileUsed(filename, code, fileId);
            Token[] tokenList = Tokenizer.Tokenize(filename, code, fileId, true);
            TokenStream tokens = new TokenStream(tokenList, filename);

            List<Executable> executables = new List<Executable>();

            List<string> namespaceImportsBuilder = new List<string>();

            tokens.InsertTokens(this.GetImplicitCoreImport());

            Library activeLibrary = libraryName == null ? null : this.LibraryManager.GetLibraryFromName(libraryName);
            if (libraryName != null && libraryName != "Core")
            {
                activeLibrary.AddLibraryDependency(this.LibraryManager.GetLibraryFromName("Core"));
            }

            while (tokens.HasMore && tokens.IsNext(this.Keywords.IMPORT))
            {
                ImportStatement importStatement = this.ExecutableParser.Parse(tokens, false, true, true, null) as ImportStatement;
                if (importStatement == null) throw new Exception();
                namespaceImportsBuilder.Add(importStatement.ImportPath);
                List<Executable> libraryEmbeddedCode = new List<Executable>();
                Library library = this.LibraryManager.ImportLibrary(this, importStatement.FirstToken, importStatement.ImportPath, libraryEmbeddedCode);
                if (library == null)
                {
                    this.unresolvedImports.Add(importStatement);
                }
                else
                {
                    if (activeLibrary != null)
                    {
                        activeLibrary.AddLibraryDependency(library);
                    }
                    executables.AddRange(libraryEmbeddedCode);
                }
            }

            string[] namespaceImports = namespaceImportsBuilder.ToArray();

            while (tokens.HasMore)
            {
                Executable executable = this.ExecutableParser.Parse(tokens, false, true, true, null);

                if (executable is ImportStatement)
                {
                    throw new ParserException(
                        executable.FirstToken,
                        this.Locale.Strings.Get("ALL_IMPORTS_MUST_OCCUR_AT_BEGINNING_OF_FILE"));
                }

                executable.NamespacePrefixSearch = namespaceImports;
                executable.LibraryName = libraryName;

                if (executable is Namespace)
                {
                    ((Namespace)executable).GetFlattenedCode(executables, namespaceImports, libraryName);
                }
                else
                {
                    executables.Add(executable);
                }
            }

            return executables.ToArray();
        }

        internal static bool IsInteger(string value)
        {
            foreach (char c in value)
            {
                if (c < '0' || c > '9')
                {
                    return false;
                }
            }
            return true;
        }

        internal void VerifyIdentifier(Token token)
        {
            if (!IsValidIdentifier(token.Value))
            {
                throw new ParserException(token, "Identifier expected.");
            }
        }

        private readonly HashSet<string> RESERVED_KEYWORDS = new HashSet<string>();

        internal bool IsReservedKeyword(string value)
        {
            return RESERVED_KEYWORDS.Contains(value);
        }

        private static readonly HashSet<char> IDENTIFIER_CHARS = new HashSet<char>("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_$".ToCharArray());
        internal bool IsValidIdentifier(string value)
        {
            if (IsReservedKeyword(value))
            {
                return false;
            }

            if (value[0] >= '0' && value[0] <= '9') return false;

            foreach (char c in value)
            {
                if (!IDENTIFIER_CHARS.Contains(c))
                {
                    return false;
                }
            }
            return true;
        }

        internal static IList<Executable> ParseBlock(Parser parser, TokenStream tokens, bool bracketsRequired, Executable owner)
        {
            List<Executable> output = new List<Executable>();

            if (tokens.PopIfPresent("{"))
            {
                while (!tokens.PopIfPresent("}"))
                {
                    output.Add(parser.ExecutableParser.Parse(tokens, false, true, false, owner));
                }
            }
            else
            {
                if (bracketsRequired)
                {
                    tokens.PopExpected("{"); // throws with reasonable exception message.
                }

                if (tokens.PopIfPresent(";"))
                {
                    return output;
                }

                output.Add(parser.ExecutableParser.Parse(tokens, false, true, false, owner));
            }
            return output;
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

        private List<string> namespaceStack = new List<string>();

        public void PushNamespacePrefix(string value)
        {
            this.namespaceStack.Add(value);
            this.CurrentNamespace = string.Join(".", this.namespaceStack);
        }

        public void PopNamespacePrefix()
        {
            this.namespaceStack.RemoveAt(this.namespaceStack.Count - 1);
            this.CurrentNamespace = string.Join(".", this.namespaceStack);
        }

        public string CurrentNamespace { get; private set; }
        public int ValueStackDepth { get; set; }
    }
}
