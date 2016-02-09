using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibraryConfig;
using Core.Platforms;

namespace Core
{
	public class Config : ILibraryConfig
	{
		private Dictionary<PlatformId, INativeTranslator> nativeTranslators;

		public Config()
		{
			// TODO: right now the only good use for this is a reference template for creating native translators in other libraries.
			// There isn't actually a good reason to only define print in lib_core as $_print is actually used in other places in the
			// VM. For example, printing stack traces. Once other libraries use native translators, remove this and put $_print back
			// the way it was.
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

		public Dictionary<string, string> GetSupplementalTranslatedCode()
		{
			return new Dictionary<string, string>()
			{
				{ "CoreLibHelper", ReadFile("CoreLibHelper.cry") },
			};
		}

		internal INativeTranslator GetTranslator(IPlatform platform)
		{
			INativeTranslator output = null;
			if (this.nativeTranslators.TryGetValue(platform.PlatformId, out output))
			{
				return output;
			}
			throw new Exception("There is no Core support for " + platform.PlatformId); // which would be bad.
		}

		public string GetEmbeddedCode()
		{
			return ReadFile("embed.cry");
		}

		public string GetTranslationCode(IPlatform platform, string functionName)
		{
			return ReadFile("Translation/" + functionName + ".cry");
		}

		public string TranslateNativeInvocation(IPlatform platform, string functionName, object[] args)
		{
			INativeTranslator nativeTranslator = this.GetTranslator(platform);

			switch (functionName)
			{
				case "$_lib_core_print": return nativeTranslator.TranslatePrint(platform, args[0]);
				default:
					throw new Exception();
			}
		}

		private static string ReadFile(string path)
		{
			return LibraryUtil.ReadEmbeddedTextResource(typeof(Config).Assembly, path);
		}
	}
}
