namespace Pastel.Transpilers
{
    public interface ILibraryNativeInvocationTranslator
    {
        string LibraryID { get; }
        string UniquePrefix { get; }

        void TranslateInvocation(
            Pastel.Transpilers.TranspilerContext sb,
            AbstractTranslator translator,
            string functionName,
            Pastel.Nodes.Expression[] args,
            Pastel.Token throwToken);
    }
}
