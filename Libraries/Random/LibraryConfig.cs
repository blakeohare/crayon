using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crayon;
using Crayon.ParseTree;

namespace Random
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

		public string GetTranslationCode(string functionName)
		{
			return ReadFile("Translation/" + functionName + ".cry");
		}

		public string TranslateNativeInvocation(ExpressionTranslator translator, string functionName, Expression[] args)
		{
			throw new Exception();
		}
	}
}
