using System;
using System.Collections.Generic;
using Crayon;
using Crayon.ParseTree;

namespace Game
{
	public class LibraryConfig : ILibraryConfig
	{
		private static string ReadFile(string path)
		{
			return Util.ReadFileInternally(typeof(LibraryConfig).Assembly, path);
		}

		public string GetEmbeddedCode()
		{
			return ReadFile("embed.cry");
		}

		private static bool ScreenBlocksExecution(PlatformId platform)
		{
			switch (platform)
			{
				case PlatformId.CSHARP_OPENTK:
				case PlatformId.JAVA_ANDROID:
				case PlatformId.JAVA_AWT:
					return true;

				case PlatformId.JAVASCRIPT_CANVAS:
				case PlatformId.PYTHON_PYGAME:
					return false;

				case PlatformId.C_OPENGL:
				default:
					throw new Exception();
			}
		}

		public string GetTranslationCode(LanguageId language, PlatformId platform, string functionName)
		{
			string file = ReadFile("Translation/" + functionName + ".cry");
			file = Constants.DoReplacements(file, new Dictionary<string, string>()
			{
				{ "SCREEN_BLOCKS_EXECUTION", ScreenBlocksExecution(platform) ? "true" : "false" },
			});
			return file;
		}

		public string TranslateNativeInvocation(ExpressionTranslator translator, string functionName, Expression[] args)
		{
			throw new Exception();
		}

		public Dictionary<string, string> GetSupplementalTranslatedCode()
		{
			return new Dictionary<string, string>()
			{
				{ "GameLibHelper", Util.ReadFileInternally(typeof(LibraryConfig).Assembly, "GameLibHelper.cry") },
			};
		}
	}
}
