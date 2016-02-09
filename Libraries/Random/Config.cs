using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibraryConfig;

namespace Random
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

		public string GetTranslationCode(IPlatform platform, string functionName)
		{
			return ReadFile("Translation/" + functionName + ".cry");
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
