using System;
using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon
{
	internal class Parser
	{
		private int fileIdCounter = 0;
		private int intCounter = 0;

		public bool IsInClass { get; set; }

		// TODO: why isn't this static?
		private Dictionary<string, FrameworkFunction> ffLookup = new Dictionary<string, FrameworkFunction>();

		public int GetNextInt()
		{
			return ++intCounter;
		}

		private Dictionary<string, Dictionary<string, int>> stringSwitchLookups = new Dictionary<string, Dictionary<string, int>>();
		private Dictionary<string, Dictionary<int, int>> intListLookups = new Dictionary<string, Dictionary<int, int>>();
		private Dictionary<string, int> explicitMaxes = new Dictionary<string, int>();
		private Dictionary<string, int> defaultCaseIds = new Dictionary<string, int>();

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

		private Dictionary<string, int> floats = new Dictionary<string, int>();
		public int GetFloatConstant(double value)
		{
			string key = Util.FloatToString(value);

			if (!floats.ContainsKey(key))
			{
				floats[key] = floats.Count + 1;
			}
			return floats[key];
		}

		private Dictionary<string, int> strings = new Dictionary<string, int>();
		public int GetStringConstant(string value)
		{
			if (!strings.ContainsKey(value))
			{
				strings[value] = strings.Count + 1;
			}

			return strings[value];
		}

		public string[] GetFloatsById()
		{
			return GetStringsByIdImpl(this.floats);
		}

		public string[] GetStringsById()
		{
			return GetStringsByIdImpl(this.strings);
		}

		private string[] GetStringsByIdImpl(Dictionary<string, int> lookup)
		{
			List<string> output = new List<string>();
			foreach (string value in lookup.Keys)
			{
				int id = lookup[value];
				while (output.Count <= id)
				{
					output.Add(null);
				}
				output[id] = value;
			}
			return output.ToArray();
		}

		public string[] GetFloatList()
		{
			List<string> output = new List<string>();
			foreach (string item in this.GetFloatsById())
			{
				if (item == null)
				{
					output.Add("null");
				}
				else
				{
					output.Add("new Value(Types.FLOAT, " + item + ")");
				}
			}
			return output.ToArray();
		}

		public string[] GetEscapedStringList()
		{
			List<string> output = new List<string>();
			foreach (string item in this.GetStringsById())
			{
				if (item == null)
				{
					output.Add("null");
				}
				else
				{
					string value = "\"";

					foreach (char c in item)
					{
						switch (c)
						{
							case '\"': value += "\\\""; break;
							case '\n': value += "\\n"; break;
							case '\r': value += "\\r"; break;
							case '\t': value += "\\t"; break;
							case '\0': value += "\\0"; break;
							case '\\': value += "\\\\"; break;
							default: value += c; break;
						}
					}

					value += "\"";
					output.Add(value);
				}
			}
			return output.ToArray();
		}

		private Dictionary<string, int> identifiers = new Dictionary<string, int>();
		public int GetId(string identifier)
		{
			if (!this.identifiers.ContainsKey(identifier))
			{
				this.identifiers[identifier] = this.identifiers.Count + 1;
			}

			return this.identifiers[identifier];
		}

		public string[] GetIdentifierLookup()
		{
			List<string> output = new List<string>() { "" };
			foreach (string key in this.identifiers.Keys)
			{
				int index = this.identifiers[key];
				while (output.Count < index + 1)
				{
					output.Add(null);
				}
				output[index] = key;
			}
			return output.ToArray();
		}

		internal PlatformTarget Mode { get; private set; }
		public bool IsTranslateMode { get { return this.Mode != PlatformTarget.ByteCode; } }
		public bool IsByteCodeMode { get { return this.Mode == PlatformTarget.ByteCode; } }

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

		private string folder;

		public Parser(PlatformTarget mode, string rootFolder)
		{
			this.Mode = mode;
			this.folder = rootFolder;
			this.IsInClass = false;

			foreach (object name in Enum.GetValues(typeof(FrameworkFunction)))
			{
				FrameworkFunction ff = (FrameworkFunction)name;
				this.ffLookup[ff.ToString().ToLowerInvariant()] = ff;
			}
		}

		public FrameworkFunction GetFrameworkFunction(Token token, string name)
		{
			if ((name.StartsWith("_") && this.IsByteCodeMode) ||
				!this.ffLookup.ContainsKey(name))
			{
				throw new ParserException(token, "Framework function by this name was not found.");
			}

			return this.ffLookup[name];
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
			Executable[] output = ParseImport(".", filename, contents, new HashSet<string>());
			return ResolveCode(output);
		}

		public Executable[] ParseRoot(string rootFolder)
		{
			string fileName = "start.cry";
			Executable[] output = ParseImport(rootFolder, fileName, null, new HashSet<string>());
			return ResolveCode(output);
		}

		public Executable[] ParseImport(string rootFolder, string filename, string codeOverride, HashSet<string> pathOfFilesRelativeToRoot)
		{
			if (pathOfFilesRelativeToRoot.Contains(filename))
			{
				throw new Exception("File imported multiple times: '" + filename + "'");
			}
			pathOfFilesRelativeToRoot.Add(filename);

			int fileId = fileIdCounter++;
			string code = codeOverride ?? Util.ReadFileExternally(System.IO.Path.Combine(rootFolder, filename), true);
			this.RegisterFileUsed(filename, code, fileId);
			TokenStream tokens = Tokenizer.Tokenize(filename, code, fileId);

			Dictionary<string, StructDefinition> structureDefinitions = new Dictionary<string, StructDefinition>();
			Dictionary<string, Expression> constantDefinitions = new Dictionary<string, Expression>();

			List<Executable> executables = new List<Executable>();
			while (tokens.HasMore)
			{
				Executable executable = ExecutableParser.Parse(tokens, false, true, true);
				if (executable is ImportStatement)
				{
					ImportStatement importStatement = (ImportStatement)executable;
					string filePath = importStatement.FilePath;
					Executable[] importedCode = this.ParseImport(rootFolder, filePath, null, pathOfFilesRelativeToRoot);
					executables.AddRange(importedCode);
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
		public string GetVariableName(string originalName)
		{
			return "v_" + originalName;
		}

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
