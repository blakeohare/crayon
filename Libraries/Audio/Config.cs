using System;
using System.Collections.Generic;
using LibraryConfig;

namespace Audio
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
			switch (translator.PlatformId)
			{
				case PlatformId.CSHARP_OPENTK:
					switch (functionName)
					{
						case "$_lib_audio_is_supported":
							return "TranslationHelper.AlwaysTrue()";
						default:
							throw new ArgumentException();
					}

				case PlatformId.PYTHON_PYGAME:
					switch (functionName)
					{
						case "$_lib_audio_is_supported":
							return "_always_true()";
						default:
							throw new ArgumentException();
					}

				case PlatformId.JAVA_AWT:
					switch (functionName)
					{
						case "$_lib_audio_is_supported":
							return "TranslationHelper.alwaysFalse()";
						default:
							throw new ArgumentException();
					}

				case PlatformId.JAVA_ANDROID:
					switch (functionName)
					{
						case "$_lib_audio_is_supported":
							return "TranslationHelper.AlwaysFalse()";
						default:
							throw new ArgumentException();
					}

				case PlatformId.C_OPENGL:
					switch (functionName)
					{
						case "$_lib_audio_is_supported":
							return "_TODO_AlwaysFalse()";
						default:
							throw new ArgumentException();
					}

				case PlatformId.JAVASCRIPT_CANVAS:
					switch (functionName)
					{
						case "$_lib_audio_is_supported":
							return "R.isAudioSupported()";
						default:
							throw new ArgumentException();
					}

				default:
					throw new ArgumentException();
			}
		}

		public Dictionary<string, string> GetSupplementalTranslatedCode()
		{
			return new Dictionary<string, string>()
			{
				{ "AudioLibHelper", ReadFile("AudioLibHelper.cry") },
			};
		}
	}
}
