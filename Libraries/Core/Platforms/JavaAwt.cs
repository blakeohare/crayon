namespace Core.Platforms
{
	class JavaAwt : INativeTranslator
	{
		public string TranslatePrint(LibraryConfig.IPlatform platform, object value)
		{
			return "System.out.println(" + platform.Translate(value) + ")";
		}
	}
}
