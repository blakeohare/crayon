using System;
using Crayon;
using System.Collections.Generic;
using Crayon.ParseTree;

namespace GFX
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

		private static bool IsOpenGlBased(PlatformId platform)
		{
			switch (platform)
			{
				case PlatformId.C_OPENGL:
				case PlatformId.CSHARP_OPENTK:
				case PlatformId.JAVA_ANDROID:
					return true;
				case PlatformId.JAVA_AWT:
				case PlatformId.JAVASCRIPT_CANVAS:
				case PlatformId.PYTHON_PYGAME:
					return false;
				default:
					throw new Exception();
			}
		}

		public string GetTranslationCode(LanguageId language, PlatformId platform, string functionName)
		{
			string file = ReadFile("Translation/" + functionName + ".cry");
			file = Constants.DoReplacements(file, new Dictionary<string, string>()
			{
				{ "IS_OPEN_GL_BASED", IsOpenGlBased(platform) ? "true" : "false" },
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
				{ "GfxLibHelper", Util.ReadFileInternally(typeof(LibraryConfig).Assembly, "GfxLibHelper.cry") },
			};
		}
	}
}
