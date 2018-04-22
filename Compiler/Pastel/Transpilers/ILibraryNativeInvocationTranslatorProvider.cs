namespace Pastel.Transpilers
{
    public interface ILibraryNativeInvocationTranslatorProvider
    {
        ILibraryNativeInvocationTranslator GetTranslator(

            // Name of the library
            string libraryName);
    }
}
