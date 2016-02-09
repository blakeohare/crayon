namespace Core.Platforms
{
	class PythonPyGame : INativeTranslator
	{
		public string TranslatePrint(LibraryConfig.IPlatform platform, object value)
		{
			return "print(" + platform.Translate(value) + ")";
		}
	}
}
