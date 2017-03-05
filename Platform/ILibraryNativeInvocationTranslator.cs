using System.Text;

namespace Platform
{
    public interface ILibraryNativeInvocationTranslator
    {
        void TranslateInvocation(
            StringBuilder sb,
            AbstractTranslator translator,
            string functionName,
            Pastel.Nodes.Expression[] args,
            Pastel.Token throwToken);
    }
}
