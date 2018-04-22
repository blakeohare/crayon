namespace Platform
{
    public interface ILibraryNativeInvocationTranslator
    {
        string LibraryID { get; }
        LibraryForExport Library { get; }

        void TranslateInvocation(
            Pastel.Transpilers.TranspilerContext sb,
            AbstractTranslator translator,
            string functionName,
            Pastel.Nodes.Expression[] args,
            Pastel.Token throwToken);
    }
}
