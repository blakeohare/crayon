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
			ByteBuffer byteBuffer = bcc.ByteBuffer;
			Token[] tokenData = byteBuffer.ToTokenList().ToArray();
			
			string usercode = ByteCodeEncoder.Encode(byteBuffer);

			string switchTables = this.parser.GetSwitchLookupCode();

			// TODO: once you start adding more platforms, do this in a more reasonable streamlined way. 
			bool isAsync = this.parser.Mode == PlatformTarget.JavaScript_Browser;
			bool isAutoload = this.parser.Mode == PlatformTarget.Python_PyGame;
			interpreter = interpreter.Replace("%%%PLATFORM_IS_ASYNC%%%", isAsync ? "true" : "false");
			interpreter = interpreter.Replace("%%%PLATFORM_IS_AUTOLOAD%%%", isAutoload ? "true" : "false");

			foreach (PrimitiveMethods primitiveMethod in Enum.GetValues(typeof(PrimitiveMethods)).Cast<PrimitiveMethods>())
			{
				interpreter = interpreter.Replace("%%%PRIMITIVE_METHOD_" + primitiveMethod.ToString() + "%%%", "" + (int)primitiveMethod);
			}

			// byte code must be added last to make accidental replacements above impossible.
			interpreter = interpreter.Replace("%%%USER_COMPILED_BYTE_CODE%%%", usercode);

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
