using Pastel.Nodes;

namespace Pastel.Transpilers
{
    public interface ILibraryNativeInvocationTranslator
    {
        string LibraryID { get; }
        string UniquePrefix { get; }

        void TranslateInvocation(
            TranspilerContext sb,
            string functionName,
            Expression[] args,
            Token throwToken);
    }
}
