using System;
using System.Collections.Generic;
using System.Linq;

namespace Crayon
{
	internal class Compiler
	{
		private string rootFolder;

		private Parser parser;
		private Crayon.Translator.AbstractTranslator platformTranslator;
		
		public Crayon.Translator.AbstractPlatformImplementation PlatformImplementation { get; private set; }

		private Parser userParser;
		private ByteCodeCompiler byteCodeCompiler = new ByteCodeCompiler();

		public Crayon.Translator.AbstractTranslator Translator { get { return this.platformTranslator; } }

		public Compiler(PlatformTarget mode, bool generateMinCode, string rootFolder, string jsFolderPrefix)
		{
			this.parser = new Parser(mode, null, generateMinCode);
			this.rootFolder = rootFolder;

			Crayon.Translator.AbstractPlatformImplementation platform;
			Crayon.Translator.AbstractTranslator translator;

			switch (mode)
			{
				case PlatformTarget.Python_PyGame:
					platform = new Translator.Python.PyGame.PyGameImplementation(generateMinCode);
					translator = new Translator.Python.PythonTranslator(parser, generateMinCode, platform);
					break;

				case PlatformTarget.JavaScript_Browser:
					platform = new Translator.JavaScript.Browser.BrowserImplementation(generateMinCode, jsFolderPrefix);
					translator = new Translator.JavaScript.JavaScriptTranslator(parser, generateMinCode, platform);
					break;

				default:
					throw new Exception("Invalid platform target.");
			}

			this.platformTranslator = translator;
			this.PlatformImplementation = platform;

			this.userParser = new Parser(PlatformTarget.ByteCode, rootFolder, false);
		}

		public string Compile()
		{
			string interpreter = Util.ReadFileInternally("Translator/Interpreter.cry");

			foreach (FrameworkFunction ff in Enum.GetValues(typeof(FrameworkFunction)).Cast<FrameworkFunction>())
			{
				interpreter = interpreter.Replace("%%%FF_" + ff.ToString() + "%%%", ((int)ff).ToString());
			}

			foreach (Types t in Enum.GetValues(typeof(Types)).Cast<Types>())
			{
				interpreter = interpreter.Replace("%%%TYPE_ID_" + t.ToString() + "%%%", ((int)t).ToString());
			}

			ParseTree.Executable[] userCode = this.userParser.ParseRoot(this.rootFolder);
			ByteCodeCompiler bcc = new ByteCodeCompiler();
			bcc.GenerateByteCode(this.userParser, userCode);
			int[][] byteCode = bcc.ByteCode;
			Token[] tokenData = bcc.TokenData;
			string[] fileById = this.userParser.GetFilesById();

			List<string> formattedByteCode = new List<string>();
			List<string> formattedRow = new List<string>();
			int i;
			foreach (int[] row in byteCode)
			{
				formattedRow.Add("[");
				for (i = 0; i < row.Length; ++i)
				{
					if (i == 0)
					{
						formattedRow.Add("OpCodes." + ((OpCode)row[i]).ToString());
					}
					else
					{
						formattedRow.Add(",");
						formattedRow.Add(row[i].ToString());
					}
				}
				formattedRow.Add("]");
				formattedByteCode.Add(string.Join("", formattedRow));
				formattedRow.Clear();
			}

			string usercode = string.Join(",\r\n", formattedByteCode);

			System.IO.File.WriteAllText("C:\\users\\blake\\desktop\\debug.txt",
				usercode
					.Replace("OpCodes.", "")
					.Replace("[", "")
					.Replace("],", "")
					.Replace(",", "  ")
					.Replace("]", ""));

			// This of course assumes all programming languages that I port to have the same standard system of escape sequences.
			string identifiers = "'" + string.Join("', '", this.userParser.GetIdentifierLookup()) + "'";
			string stringTable = string.Join(", ", this.userParser.GetEscapedStringList());
			string floatTable = string.Join(", ", this.userParser.GetFloatList());
			string tokenTable = string.Join(", ", tokenData.Select<Token, string>(token => token == null ? "null" : ("[" + token.Line + ", " + token.Col + ", " + token.FileID + "]")));
			string fileTable = "\"" + string.Join("\", \"", this.userParser.GetFilesById().Select<string, string>(contents => contents == null ? "" : contents.Replace("\\", "\\\\").Replace("\n", "\\n").Replace("\t", "\\t").Replace("\"", "\\\""))) + "\"";
			string switchTables = this.parser.GetSwitchLookupCode();

			interpreter = interpreter.Replace("%%%USER_COMPILED_BYTE_CODE%%%", usercode);
			interpreter = interpreter.Replace("%%%IDENTIFIER_TABLE%%%", identifiers);
			interpreter = interpreter.Replace("%%%STRING_TABLE%%%", stringTable);
			interpreter = interpreter.Replace("%%%FLOAT_TABLE%%%", floatTable);
			interpreter = interpreter.Replace("%%%TOKEN_DATA%%%", tokenTable);
			interpreter = interpreter.Replace("%%%FILE_DATA%%%", fileTable);

			// TODO: once you start adding more platforms, do this in a more reasonable streamlined way. 
			bool isAsync = this.parser.Mode == PlatformTarget.JavaScript_Browser;
			bool isAutoload = this.parser.Mode == PlatformTarget.Python_PyGame;
			interpreter = interpreter.Replace("%%%PLATFORM_IS_ASYNC%%%", isAsync ? "true" : "false");
			interpreter = interpreter.Replace("%%%PLATFORM_IS_AUTOLOAD%%%", isAutoload ? "true" : "false");

			foreach (PrimitiveMethods primitiveMethod in Enum.GetValues(typeof(PrimitiveMethods)).Cast<PrimitiveMethods>())
			{
				interpreter = interpreter.Replace("%%%PRIMITIVE_METHOD_" + primitiveMethod.ToString() + "%%%", "" + (int)primitiveMethod);
			}

			Crayon.ParseTree.Executable[] lines = this.parser.ParseInternal("interpreter.cry", interpreter);
			Crayon.ParseTree.Executable[] switchLookups = this.parser.ParseInternal("switch_lookups.cry", this.parser.GetSwitchLookupCode());

			List<Crayon.ParseTree.Executable> combined = new List<ParseTree.Executable>();
			combined.AddRange(switchLookups);
			combined.AddRange(lines);
			lines = combined.ToArray();

			return this.platformTranslator.DoTranslationOfInterpreterClassWithEmbeddedByteCode(this.parser, lines);
		}
	}
}
