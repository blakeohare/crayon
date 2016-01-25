using System;
using System.Collections.Generic;
using System.Linq;
using Crayon.ParseTree;

namespace Crayon
{
	internal class InterpreterCompiler
	{
		// Order in which the files are compiled.
		private static readonly string[] FILES = new string[] {

			// These 3 must go first
			"Structs.cry",
			"Constants.cry",
			"Globals.cry",

			// These are just piles of functions so they can be compiled in any order.
			"BinaryOpsUtil.cry",
			"ByteCodeLoader.cry",
			"ClassInitializer.cry",
			"GamepadManager.cry",
			"Graphics.cry",
			"Interpreter.cry",
			"IOManager.cry",
			"NetworkManager.cry",
			"OpenGlPipeline.cry", // conditionally excluded below based on platform.
			"PrimitiveMethods.cry",
			"Runner.cry",
			"SoundManager.cry",
			"TypesUtil.cry",
			"ValueUtil.cry",
		};

		private AbstractPlatform platform;
		private Parser interpreterParser;

		public InterpreterCompiler(AbstractPlatform platform, SystemLibraryManager sysLibMan)
		{
			this.platform = platform;
			this.interpreterParser = new Parser(platform, null, sysLibMan);
		}

		public Dictionary<string, Executable[]> Compile()
		{
			Dictionary<string, Executable[]> output = new Dictionary<string, Executable[]>();

			Dictionary<string, string> replacements = this.BuildReplacementsDictionary();

			foreach (string file in FILES)
			{
				string fileId = file.Split('.')[0];

				if (fileId.StartsWith("OpenGl") && !this.platform.IsOpenGlBased)
				{
					continue;
				}

				string code = Util.ReadFileInternally("InterpreterSource/" + file);

				code = Constants.DoReplacements(code, replacements);

				Executable[] lines = this.interpreterParser.ParseInterpreterCode(file, code);
				if (lines.Length > 0)
				{
					output[fileId] = lines.ToArray();
				}
			}

			string switchLookupCode = this.interpreterParser.GetSwitchLookupCode().Trim();
			if (switchLookupCode.Length > 0)
			{
				output["SwitchLookups"] = this.interpreterParser.ParseInterpreterCode("SwitchLookups.cry", switchLookupCode);
			}

			return output;
		}

		public Dictionary<string, string> BuildReplacementsDictionary()
		{
			Dictionary<string, string> replacements = new Dictionary<string, string>();
			replacements.Add("PLATFORM_IS_ASYNC", this.platform.IsAsync ? "true" : "false");
			replacements.Add("PLATFORM_SUPPORTS_LIST_CLEAR", this.platform.SupportsListClear ? "true" : "false");
			replacements.Add("STRONGLY_TYPED", this.platform.IsStronglyTyped ? "true" : "false");
			replacements.Add("USE_FIXED_LENGTH_ARG_CONSTRUCTION", this.platform.UseFixedListArgConstruction ? "true" : "false");
			replacements.Add("IMAGES_LOAD_INSTANTLY", this.platform.ImagesLoadInstantly ? "true" : "false");
			replacements.Add("IS_OPEN_GL_BASED", this.platform.IsOpenGlBased ? "true" : "false");
			replacements.Add("IS_OPEN_GL_NEW_STYLE", (this.platform.IsOpenGlBased && this.platform.OpenGlTranslator.IsNewStyle) ? "true" : "false");
			replacements.Add("IS_GAMEPAD_SUPPORTED", this.platform.IsGamepadSupported ? "true" : "false");
			replacements.Add("IS_ARRAY_SAME_AS_LIST", this.platform.IsArraySameAsList ? "true" : "false");
			replacements.Add("GENERATED_TILE_DIRECTORY", this.platform.GeneratedFilesFolder + "/spritesheets");
			replacements.Add("PLATFORM_SHORT_ID", this.platform.PlatformShortId);
			replacements.Add("LIBRARY_FUNCTION_BIG_SWITCH_STATEMENT", this.platform.LibraryBigSwitchStatement);
			return replacements;
		}

		public StructDefinition[] GetStructDefinitions()
		{
			return this.interpreterParser.GetStructDefinitions();
		}
	}
}
