namespace Core.Platforms
{
	class CSharpOpenTk : INativeTranslator
	{
		public string TranslatePrint(LibraryConfig.IPlatform platform, object value)
		{
			return "System.Console.WriteLine(" + platform.Translate(value) + ")";
		}
	}
}
