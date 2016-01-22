using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crayon;
using Core.Platforms;
using Crayon.ParseTree;

namespace Core
{
	public class LibraryConfig : ILibraryConfig
	{
		private Dictionary<PlatformId, INativeTranslator> nativeTranslators;

		public LibraryConfig()
		{
			this.nativeTranslators = new Dictionary<PlatformId, INativeTranslator>()
			{
				{ PlatformId.C_OPENGL, new COpenGl() },
				{ PlatformId.CSHARP_OPENTK, new CSharpOpenTk() },
				{ PlatformId.JAVA_ANDROID, new JavaAndroid() },
				{ PlatformId.JAVA_AWT, new JavaAwt() },
				{ PlatformId.JAVASCRIPT_CANVAS, new JavaScriptCanvas() },
				{ PlatformId.PYTHON_PYGAME, new PythonPyGame() },
			};
		}

		internal INativeTranslator GetTranslator(ExpressionTranslator exprTranslator)
		{
			INativeTranslator output = null;
			if (this.nativeTranslators.TryGetValue(exprTranslator.Platform, out output))
			{
				return output;
			}
			throw new Exception("There is no Core support for " + exprTranslator.Platform); // which would be bad.
		}

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
			INativeTranslator nativeTranslator = this.GetTranslator(translator);

			switch (functionName)
			{
				case "$_lib_core_print": return nativeTranslator.TranslatePrint(translator, args[0]);
				default:
					throw new Exception();
			}
		}
	}
}
