using System;
using System.Collections.Generic;
using System.Linq;
using Crayon.ParseTree;

namespace Crayon
{
	internal class Parser
	{
		public Parser(AbstractPlatform platform, BuildContext buildContext, SystemLibraryManager sysLibMan)
		{
			this.NullablePlatform = platform;
			this.IsTranslateMode = platform != null;
			this.CurrentClass = null;
			this.CurrentSystemLibrary = null;
			this.BuildContext = buildContext;
			this.VariableIds = new VariableIdAllocator();
			this.SystemLibraryManager = sysLibMan ?? new SystemLibraryManager();
			this.CurrentNamespace = "";
			this.NamespacePrefixLookupForCurrentFile = new List<string>();
			this.ConstantAndEnumResolutionState = new Dictionary<Executable, int>();
		}

		public Dictionary<Executable, int> ConstantAndEnumResolutionState { get; private set; }

		private int functionIdCounter = 0;
		private int fileIdCounter = 0;

		public int GetNextFunctionId()
		{
			return ++this.functionIdCounter;
		}

		private Dictionary<ClassDefinition, int> classIdsByInstance = new Dictionary<ClassDefinition, int>();

		// HACK ALERT - Forgive me father for I have sinned.
		// I need an access-anywhere boolean flag to determine if the parser is running in translate mode.
		// Syntax parsing is currently stateless. Which is nice. In an ideal world.
		// One day I will undo this for a more reasonable solution.
		public static bool IsTranslateMode_STATIC_HACK { get; set; }

		public VariableIdAllocator VariableIds { get; private set; }

		public static string CurrentSystemLibrary_STATIC_HACK { get; set; }
		public string CurrentSystemLibrary { get; set; }

		public SystemLibraryManager SystemLibraryManager { get; private set; }

		public ClassDefinition CurrentClass { get; set; }

		public bool MainFunctionHasArg { get; set; }

		public bool IsInClass { get { return this.CurrentClass != null; } }

		public BuildContext BuildContext { get; private set; }

		public List<string> NamespacePrefixLookupForCurrentFile { get; private set; }

		public bool PreserveTranslationComments
		{
			get { return this.NullablePlatform == null ? false : !this.NullablePlatform.IsMin; }
		}

		public bool RemoveBreaksFromSwitch { get { return this.NullablePlatform == null ? false : this.NullablePlatform.RemoveBreaksFromSwitch; } }

		public LiteralLookup LiteralLookup { get { return this.literalLookup; } }
		private LiteralLookup literalLookup = new LiteralLookup();
		public int GetId(string name) { return this.literalLookup.GetNameId(name); }
		public int GetStringConstant(string value) { return this.literalLookup.GetStringId(value); }
		public int GetFloatConstant(double value) { return this.literalLookup.GetFloatId(value); }
		public int GetBoolConstant(bool value) { return this.literalLookup.GetBoolId(value); }
		public int GetIntConstant(int value) { return this.literalLookup.GetIntId(value); }
		public int GetNullConstant() { return this.literalLookup.GetNullId(); }

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

		public int RegisterByteCodeSwitch(Token switchToken, Dictionary<int, int> chunkIdsToOffsets, Dictionary<int, int> integersToChunkIds, Dictionary<string, int> stringsToChunkIds, bool isIntegerSwitch)
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

		public bool IsTranslateMode { get; private set; }
		public bool IsByteCodeMode { get { return !this.IsTranslateMode; } }

		private Dictionary<string, EnumDefinition> enumDefinitions = new Dictionary<string, EnumDefinition>();
		private Dictionary<string, StructDefinition> structDefinitions = new Dictionary<string, StructDefinition>();
		private Dictionary<string, Expression> constLookup = new Dictionary<string, Expression>();
		private HashSet<string> things = new HashSet<string>();

		private Dictionary<string, ClassDefinition> classDefinitions = new Dictionary<string, ClassDefinition>();
		private Dictionary<string, int> classDefinitionOrder = new Dictionary<string, int>();

		public ClassDefinition GetClass(string name)
		{
			if (this.classDefinitions.ContainsKey(name))
			{
				return this.classDefinitions[name];
			}
			return null;
		}

		private void VerifyNameFree(Token nameToken)
		{
			if (things.Contains(nameToken.Value))
			{
				throw new ParserException(nameToken, "This name has already been used.");
			}
		}

		public void RegisterConst(Token nameToken, Expression value)
		{
			this.VerifyNameFree(nameToken);
			this.things.Add(nameToken.Value);
			this.constLookup[nameToken.Value] = value;
		}

		public Expression GetConst(string name)
		{
			if (this.constLookup.ContainsKey(name))
			{
				return this.constLookup[name];
			}
			return null;
		}

		public AbstractPlatform NullablePlatform { get; private set; }

		public void AddEnumDefinition(EnumDefinition enumDefinition)
		{
			if (this.enumDefinitions.ContainsKey(enumDefinition.Name))
			{
				throw new ParserException(enumDefinition.FirstToken, "An enum with this name has already been defined.");
			}

			this.VerifyNameFree(enumDefinition.NameToken);

			this.enumDefinitions.Add(enumDefinition.Name, enumDefinition);
		}

		public void AddStructDefinition(StructDefinition structDefinition)
		{
			if (this.structDefinitions.ContainsKey(structDefinition.Name.Value))
			{
				throw new ParserException(structDefinition.FirstToken, "A struct with this name has already been defined.");
			}

			this.VerifyNameFree(structDefinition.Name);

			this.structDefinitions.Add(structDefinition.Name.Value, structDefinition);
		}

		public StructDefinition[] GetStructDefinitions()
		{
			return this.structDefinitions.Values.ToArray();
		}

		public StructDefinition GetStructDefinition(string name)
		{
			StructDefinition output = null;
			return this.structDefinitions.TryGetValue(name, out output) ? output : null;
		}

		public EnumDefinition GetEnumDefinition(string name)
		{
			EnumDefinition output = null;
			return this.enumDefinitions.TryGetValue(name, out output) ? output : null;
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
			TokenStream tokens = new TokenStream(Tokenizer.Tokenize(filename, contents, 0, true));
			List<Executable> output = new List<Executable>();
			while (tokens.HasMore)
			{
				output.Add(ExecutableParser.Parse(this, tokens, false, true, true, null));
			}
			return new Resolver(this, output).ResolveTranslatedCode();
		}

		private void GetCodeFilesImpl(string rootFolder, string currentFolder, Dictionary<string, string> filesOutput)
		{
			foreach (string file in System.IO.Directory.GetFiles(currentFolder))
			{
				if (file.ToLowerInvariant().EndsWith(".cry"))
				{
					string contents = System.IO.File.ReadAllText(file);
					string relativePath = file.Substring(rootFolder.Length + 1);
					filesOutput[relativePath] = contents;
				}
			}

			foreach (string directory in System.IO.Directory.GetDirectories(currentFolder))
			{
				GetCodeFilesImpl(rootFolder, directory, filesOutput);
			}
		}

		public Dictionary<string, string> GetCodeFiles(string rootFolder)
		{
			rootFolder = System.IO.Path.GetFullPath(rootFolder);
			Dictionary<string, string> output = new Dictionary<string, string>();
			this.GetCodeFilesImpl(rootFolder, rootFolder, output);
			return output;
		}

		public Executable[] ParseAllTheThings(string rootFolder)
		{
			List<Executable> output = new List<Executable>();
			Dictionary<string, string> files = this.GetCodeFiles(rootFolder);
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

		private HashSet<string> importedFiles = new HashSet<string>();

		public int GetNextFileId()
		{
			return fileIdCounter++;
		}

		private static readonly Token[] implicitCoreImport = Tokenizer.Tokenize("implicit code", "import Core;", -1, true);

		public Executable[] ParseInterpretedCode(string filename, string code, string libraryName)
		{
			int fileId = this.GetNextFileId();
			this.RegisterFileUsed(filename, code, fileId);
			Token[] tokenList = Tokenizer.Tokenize(filename, code, fileId, true);
			TokenStream tokens = new TokenStream(tokenList);

			List<Executable> executables = new List<Executable>();

			List<string> namespaceImportsBuilder = new List<string>();

			tokens.InsertTokens(implicitCoreImport);

			while (tokens.HasMore && tokens.IsNext("import"))
			{
				ImportStatement importStatement = ExecutableParser.Parse(this, tokens, false, true, true, null) as ImportStatement;
				if (importStatement == null) throw new Exception();
				namespaceImportsBuilder.Add(importStatement.ImportPath);
				Executable[] libraryEmbeddedCode = this.SystemLibraryManager.ImportLibrary(this, importStatement.FirstToken, importStatement.ImportPath);
				executables.AddRange(libraryEmbeddedCode);
			}

			string[] namespaceImports = namespaceImportsBuilder.ToArray();

			while (tokens.HasMore)
			{
				Executable executable;
				try
				{
					executable = ExecutableParser.Parse(this, tokens, false, true, true, null);
				}
				catch (EofException)
				{
					throw new ParserException(null, "Unexpected EOF encountered while parsing " + filename + ". Did you forget a closing curly brace?");
				}

				if (executable is ImportStatement)
				{
					throw new ParserException(executable.FirstToken, "All imports must occur at the beginning of the file.");
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

		internal static void VerifyIdentifier(Token token)
		{
			if (!IsValidIdentifier(token.Value))
			{
				throw new ParserException(token, "Identifier expected.");
			}
		}

		private static readonly HashSet<string> RESERVED_KEYWORDS = new HashSet<string>(
			new string[] {
				"base",
				"break",
				"case",
				"catch",
				"class",
				"const",
				"constructor",
				"continue",
				"default",
				"do",
				"else",
				"enum",
				"false",
				"field",
				"finally",
				"for",
				"function",
				"if",
				"import",
				"interface",
				"namespace",
				"new",
				"null",
				"return",
				"static",
				"switch",
				"this",
				"true",
				"try",
				"while",
			});

		internal static bool IsReservedKeyword(string value)
		{
			return RESERVED_KEYWORDS.Contains(value);
		}

		private static readonly HashSet<char> IDENTIFIER_CHARS = new HashSet<char>("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_$".ToCharArray());
		internal static bool IsValidIdentifier(string value)
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

		private Dictionary<string, int> variableNames = new Dictionary<string, int>();

		internal static IList<Executable> ParseBlock(Parser parser, TokenStream tokens, bool bracketsRequired, Executable owner)
		{
			List<Executable> output = new List<Executable>();

			if (tokens.PopIfPresent("{"))
			{
				while (!tokens.PopIfPresent("}"))
				{
					output.Add(ExecutableParser.Parse(parser, tokens, false, true, false, owner));
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

				output.Add(ExecutableParser.Parse(parser, tokens, false, true, false, owner));
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
	}
}
