using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crayon;
using Crayon.ParseTree;

namespace Math
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

		private static bool IsIntFloor(LanguageId language) {
			switch (language) {
				case LanguageId.C:
				case LanguageId.JAVA:
				case LanguageId.JAVASCRIPT:
					return true;
				case LanguageId.PYTHON:
				case LanguageId.CSHARP:
					return false;
				default:
					throw new Exception(); // Unknown platform.
			}
		}

		public string GetTranslationCode(LanguageId language, PlatformId platform, string functionName)
		{
			string file = ReadFile("Translation/" + functionName + ".cry");

			file = Crayon.Constants.DoReplacements(file, new Dictionary<string, string>()
			{
				{ "INT_IS_FLOOR", IsIntFloor(language) ? "true" : "false" },
			});

			return file;
		}

		public string TranslateNativeInvocation(ExpressionTranslator translator, string functionName, Expression[] args)
		{
			throw new Exception();
		}

		public Dictionary<string, string> GetSupplementalTranslatedCode()
		{
			return new Dictionary<string, string>();
		}
	}
}
