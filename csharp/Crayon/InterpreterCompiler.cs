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
			"Graphics.cry",
			"Interpreter.cry",
			"PrimitiveMethods.cry",
			"Runner.cry",
			"TypesUtil.cry",
			"ValueUtil.cry",
		};

		private AbstractPlatform platform;
		private Parser interpreterParser;

		public InterpreterCompiler(AbstractPlatform platform)
		{
			this.platform = platform;
			this.interpreterParser = new Parser(platform);
		}

		public Dictionary<string, Executable[]> Compile()
		{
			Dictionary<string, Executable[]> output = new Dictionary<string, Executable[]>();

			Dictionary<string, string> replacements = this.BuildReplacementsDictionary();

			foreach (string file in FILES)
			{
				string fileId = file.Split('.')[0];

				string code = Util.ReadFileInternally("InterpreterSource/" + file);

				if (code.Contains("%%%"))
				{
					foreach (string key in replacements.Keys)
					{
						code = code.Replace("%%%" + key + "%%%", replacements[key]);
					}
				}

				Executable[] lines = this.interpreterParser.ParseInternal(file, code);
				if (lines.Length > 0)
				{
					output[fileId] = lines.ToArray();
				}
			}

			string switchLookupCode = this.interpreterParser.GetSwitchLookupCode().Trim();
			if (switchLookupCode.Length > 0)
			{
				output["SwitchLookups"] = this.interpreterParser.ParseInternal("SwitchLookups.cry", switchLookupCode);
			}

			return output;
		}

		private Dictionary<string, string> BuildReplacementsDictionary()
		{
			Dictionary<string, string> replacements = new Dictionary<string, string>();

			foreach (FrameworkFunction ff in Enum.GetValues(typeof(FrameworkFunction)).Cast<FrameworkFunction>())
			{
				replacements.Add("FF_" + ff.ToString(), ((int)ff).ToString());
			}
			foreach (Types t in Enum.GetValues(typeof(Types)).Cast<Types>())
			{
				replacements.Add("TYPE_ID_" + t.ToString(), ((int)t).ToString());
			}
			foreach (PrimitiveMethods primitiveMethod in Enum.GetValues(typeof(PrimitiveMethods)).Cast<PrimitiveMethods>())
			{
				replacements.Add("PRIMITIVE_METHOD_" + primitiveMethod.ToString(), "" + (int)primitiveMethod);
			}

			replacements.Add("PLATFORM_IS_ASYNC", this.platform.IsAsync ? "true" : "false");
			replacements.Add("PLATFORM_SUPPORTS_LIST_CLEAR", this.platform.SupportsListClear ? "true" : "false");
			replacements.Add("STRONGLY_TYPED", this.platform.IsStronglyTyped ? "true" : "false");
			replacements.Add("INT_IS_FLOOR", this.platform.IntIsFloor ? "true" : "false");
			replacements.Add("USE_FIXED_LENGTH_ARG_CONSTRUCTION", this.platform.UseFixedListArgConstruction ? "true" : "false");
			replacements.Add("IMAGES_LOAD_INSTANTLY", this.platform.ImagesLoadInstantly ? "true" : "false");
			replacements.Add("SCREEN_BLOCKS_EXECUTION", this.platform.ScreenBlocksExecution ? "true" : "false");
			replacements.Add("IS_OPEN_GL_BASED", this.platform.IsOpenGlBased ? "true" : "false");
			return replacements;
		}

		public StructDefinition[] GetStructDefinitions()
		{
			return this.interpreterParser.GetStructDefinitions();
		}
	}
}
