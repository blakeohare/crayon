using System;
using System.Collections.Generic;
using LibraryConfig;

namespace Game
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

		public string GetTranslationCode(IPlatform platform, string functionName)
		{
			string file = ReadFile("Translation/" + functionName + ".cry");
			file = platform.DoReplacements(file, new Dictionary<string, string>()
			{
				{ "SCREEN_BLOCKS_EXECUTION", ScreenBlocksExecution(platform.PlatformId) ? "true" : "false" },
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
				{ "GameLibHelper", ReadFile("GameLibHelper.cry") },
			};
		}
	}
}
