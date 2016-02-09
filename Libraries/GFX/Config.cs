using System;
using System.Collections.Generic;
using LibraryConfig;

namespace GFX
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

		public string GetTranslationCode(IPlatform platform, string functionName)
		{
			string file = ReadFile("Translation/" + functionName + ".cry");
			file = platform.DoReplacements(file, new Dictionary<string, string>()
			{
				{ "IS_OPEN_GL_BASED", IsOpenGlBased(platform.PlatformId) ? "true" : "false" },
			});
			return file;
		}

		public string TranslateNativeInvocation(IPlatform translator, string functionName, object[] args)
		{
			throw new Exception();
		}

		public Dictionary<string, string> GetSupplementalTranslatedCode()
		{
			return new Dictionary<string, string>()
			{
				{ "GfxLibHelper", ReadFile("GfxLibHelper.cry") },
			};
		}
	}
}
