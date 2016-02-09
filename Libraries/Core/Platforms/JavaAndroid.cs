namespace Core.Platforms
{
	class JavaAndroid : INativeTranslator
	{
		public string TranslatePrint(LibraryConfig.IPlatform platform, object value)
		{
			return "android.util.Log.d(\"\", " + platform.Translate(value) + ")";
		}
	}
}
