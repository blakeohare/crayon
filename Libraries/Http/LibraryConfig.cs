using System;
using System.Collections.Generic;
using Crayon;
using Crayon.ParseTree;

namespace HTTP
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

		public string GetTranslationCode(LanguageId language, PlatformId platform, string functionName)
		{
			return ReadFile("Translation/" + functionName + ".cry");
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
