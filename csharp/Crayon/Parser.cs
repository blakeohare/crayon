using System;
using System.Collections.Generic;
using System.Linq;
using Crayon.ParseTree;

namespace Crayon
{
	internal class Parser
	{
		public Parser(AbstractPlatform platform, BuildContext buildContext)
		{
			this.NullablePlatform = platform;
			this.IsTranslateMode = platform != null;
			this.IsInClass = false;
			this.BuildContext = buildContext;
			this.VariableIds = new VariableIdAllocator();
		}

		private int fileIdCounter = 0;

		public VariableIdAllocator VariableIds { get; private set; }

		public bool IsInClass { get; set; }

		public BuildContext BuildContext { get; private set; }

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

		public int RegisterByteCodeSwitch(Dictionary<int, int> chunkIdsToOffsets, Dictionary<int, int> integersToChunkIds, Dictionary<string, int> stringsToChunkIds)
		{
			int switchId;
			if (integersToChunkIds.Count > 0 && stringsToChunkIds.Count == 0)
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
			else if (integersToChunkIds.Count == 0 && stringsToChunkIds.Count > 0)
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
			else
			{
				throw new Exception("Switch statement with no cases should have been optimized out.");
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

		public bool IsTranslateMode { get; private set; }
		public bool IsByteCodeMode { get { return !this.IsTranslateMode; } }

		private Dictionary<string, EnumDefinition> enumDefinitions = new Dictionary<string, EnumDefinition>();
		private Dictionary<string, StructDefinition> structDefinitions = new Dictionary<string, StructDefinition>();
		private Dictionary<string, Expression> constLookup = new Dictionary<string, Expression>();
		private HashSet<string> things = new HashSet<string>();

		private Dictionary<string, ClassDefinition> classDefinitions = new Dictionary<string, ClassDefinition>();

		public ClassDefinition GetClass(string name)
		{
			if (this.classDefinitions.ContainsKey(name))
			{
				return this.classDefinitions[name];
			}
			return null;
		}

		public void RegisterClass(ClassDefinition classDef)
		{
			string name = classDef.NameToken.Value;
			if (this.classDefinitions.ContainsKey(name))
			{
				throw new ParserException(classDef.FirstToken, "Multiple classes with the name: '" + name + "'");
			}
			this.classDefinitions[name] = classDef;
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

		public FrameworkFunction GetFrameworkFunction(Token token, string name)
		{
			if ((name.StartsWith("_") && this.IsByteCodeMode) ||
				!FrameworkFunctionUtil.FF_LOOKUP.ContainsKey(name))
			{
				throw new ParserException(token, "Framework function by this name was not found: '$" + name + "'");
			}

			return FrameworkFunctionUtil.FF_LOOKUP[name];
		}

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

		private Executable[] ResolveCode(Executable[] original)
		{
			List<Executable> output = new List<Executable>();
			foreach (Executable line in original)
			{
				output.AddRange(line.Resolve(this));
			}
			return output.ToArray();
		}

		public Executable[] ParseInternal(string filename, string contents)
		{
			Executable[] output = ParseImport(".", filename, contents, new HashSet<string>(), null);
			return ResolveCode(output);
		}

		public Executable[] ParseRoot(string rootFolder)
		{
			string fileName = "start.cry";
			Executable[] output = ParseImport(rootFolder, fileName, null, new HashSet<string>(), null);
			output = ResolveCode(output);
			return output;
		}

		public Executable[] ParseImport(string rootFolder, string filename, string codeOverride, HashSet<string> pathOfFilesRelativeToRoot, ImportStatement importStatement)
		{
			if (pathOfFilesRelativeToRoot.Contains(filename))
			{
				throw new Exception("File imported multiple times: '" + filename + "'");
			}
			pathOfFilesRelativeToRoot.Add(filename);

			int fileId = fileIdCounter++;
			string code = codeOverride;
			if (codeOverride == null)
			{
				if (importStatement != null && importStatement.IsSystemLibrary)
				{
					code = Util.ReadFileInternally("SystemLib/" + importStatement.FileToken.Value + ".cry");
				}
				else
				{
					string fullpath = System.IO.Path.Combine(rootFolder, filename);
					if (System.IO.File.Exists(fullpath))
					{
						code = Util.ReadFileExternally(fullpath, true);
					}
					else
					{
						throw new ParserException(importStatement.FirstToken, "File does not exist or is misspelled: '" + filename + "'");
					}
				}
			}
			this.RegisterFileUsed(filename, code, fileId);
			TokenStream tokens = Tokenizer.Tokenize(filename, code, fileId, true);

			Dictionary<string, StructDefinition> structureDefinitions = new Dictionary<string, StructDefinition>();
			Dictionary<string, Expression> constantDefinitions = new Dictionary<string, Expression>();

			List<Executable> executables = new List<Executable>();
			while (tokens.HasMore)
			{
				Executable executable = ExecutableParser.Parse(tokens, false, true, true);
				if (executable is ImportStatement)
				{
					ImportStatement execAsImportStatement = (ImportStatement)executable;
					string filePath = execAsImportStatement.FilePath;
					Executable[] importedCode = this.ParseImport(rootFolder, filePath, null, pathOfFilesRelativeToRoot, execAsImportStatement);
					executables.AddRange(importedCode);
				}
				else if (executable is ClassDefinition)
				{
					this.RegisterClass((ClassDefinition)executable);
					executables.Add(executable);
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
			"if else class function constructor return break continue for do while true false null this import enum switch base case default foreach try catch finally new".Split(' '));
		internal static bool IsReservedKeyword(string value)
		{
			return RESERVED_KEYWORDS.Contains(value);
		}

		private static readonly HashSet<char> IDENTIFIER_CHARS = new HashSet<char>("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_$".ToCharArray());
		internal static bool IsValidIdentifier(string value)
		{
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

		internal static IList<Executable> ParseBlock(TokenStream tokens, bool bracketsRequired)
		{
			List<Executable> output = new List<Executable>();

			if (tokens.PopIfPresent("{"))
			{
				while (!tokens.PopIfPresent("}"))
				{
					output.Add(ExecutableParser.Parse(tokens, false, true, false));
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

				output.Add(ExecutableParser.Parse(tokens, false, true, false));
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
	}
}
