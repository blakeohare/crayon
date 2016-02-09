using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibraryConfig;

namespace Math
{
	public class Config : ILibraryConfig
	{
		private static string ReadFile(string path)
		{
			return LibraryUtil.ReadEmbeddedTextResource(typeof(Config).Assembly, path);
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

		public string GetTranslationCode(IPlatform platform, string functionName)
		{
			string file = ReadFile("Translation/" + functionName + ".cry");

			file = platform.DoReplacements(file, new Dictionary<string, string>()
			{
				{ "INT_IS_FLOOR", IsIntFloor(platform.LanguageId) ? "true" : "false" },
			});

			return file;
		}

		public string TranslateNativeInvocation(IPlatform translator, string functionName, object[] args)
		{
			throw new Exception();
		}

		public Dictionary<string, string> GetSupplementalTranslatedCode()
		{
			return new Dictionary<string, string>();
		}
	}
}
