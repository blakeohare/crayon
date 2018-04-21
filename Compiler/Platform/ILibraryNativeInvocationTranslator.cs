using System.Text;

namespace Platform
{
    public interface ILibraryNativeInvocationTranslator
    {
        string LibraryID { get; }
        LibraryForExport Library { get; }

        void TranslateInvocation(
            StringBuilder sb,
            AbstractPlatform platform,
            AbstractTranslator translator,
            string functionName,
            Pastel.Nodes.Expression[] args,
            Pastel.Token throwToken);
    }
}
