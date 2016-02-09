namespace Core.Platforms
{
	class JavaScriptCanvas : INativeTranslator
	{
		public string TranslatePrint(LibraryConfig.IPlatform platform, object value)
		{
			return "R.print(" + platform.Translate(value) + ")";
		}
	}
}
