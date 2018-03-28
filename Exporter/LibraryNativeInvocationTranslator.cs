using Parser;
using Pastel.Nodes;
using Platform;
using System.Text;

namespace Exporter
{
    class LibraryNativeInvocationTranslator : ILibraryNativeInvocationTranslator
    {
        private LibraryMetadata library;

        public LibraryForExport Library { get; private set; }

        public LibraryNativeInvocationTranslator(LibraryMetadata library, LibraryForExport lfe)
        {
            this.library = library;
            this.Library = lfe;
        }

        public string LibraryID { get { return library.ID; } }

        public void TranslateInvocation(
            StringBuilder sb,
            AbstractTranslator translator,
            string functionName,
            Expression[] args,
            Pastel.Token throwToken)
        {
            try
            {
                sb.Append(LibraryExporter.Get(this.library, translator.Platform).TranslateNativeInvocation(throwToken, translator, functionName, args));
            }
            catch (System.Reflection.TargetInvocationException tie)
            {
                System.Console.WriteLine("Library translation generated an error:");
                System.Console.WriteLine(tie.InnerException.Message);
                System.Console.WriteLine(tie.InnerException.StackTrace);
                System.Environment.Exit(1);
            }
        }
    }
}
