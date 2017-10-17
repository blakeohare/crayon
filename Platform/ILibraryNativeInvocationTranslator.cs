using System.Text;

namespace Platform
{
    public interface ILibraryNativeInvocationTranslator
    {
        string LibraryName { get; }
        LibraryForExport Library { get; }

        void TranslateInvocation(
            StringBuilder sb,
            AbstractTranslator translator,
            string functionName,
            Pastel.Nodes.Expression[] args,
            Pastel.Token throwToken);
    }
}
