using Parser;
using Pastel.Nodes;
using Pastel.Transpilers;
using Platform;

namespace Exporter
{
    class LibraryNativeInvocationTranslator : ILibraryNativeInvocationTranslator
    {
        private LibraryMetadata library;
        private AbstractPlatform platform;

        public LibraryForExport Library { get; private set; }

        public LibraryNativeInvocationTranslator(LibraryMetadata library, LibraryForExport lfe, AbstractPlatform platform)
        {
            this.platform = platform;
            this.library = library;
            this.Library = lfe;
        }

        public string LibraryID { get { return library.ID; } }

        public void TranslateInvocation(
            TranspilerContext sb,
            AbstractTranslator translator,
            string functionName,
            Expression[] args,
            Pastel.Token throwToken)
        {
            try
            {
                LibraryExporter.Get(this.library, this.platform).TranslateNativeInvocation(sb, throwToken, this.platform, translator, functionName, args);
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
