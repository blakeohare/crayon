namespace Core.Platforms
{
	internal interface INativeTranslator
	{
		string TranslatePrint(LibraryConfig.IPlatform platform, object value);
	}
}
