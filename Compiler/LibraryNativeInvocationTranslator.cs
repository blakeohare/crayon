using Pastel.Nodes;
using Platform;
using System.Collections.Generic;
using System.Text;

namespace Crayon
{
    class LibraryNativeInvocationTranslator : ILibraryNativeInvocationTranslator
    {
        private LibraryMetadata library;
        private IList<string> platformInheritanceChain;

        public LibraryForExport Library { get; private set; }

        public LibraryNativeInvocationTranslator(LibraryMetadata library, LibraryForExport lfe, IList<string> platformInheritanceChain)
        {
            this.library = library;
            this.Library = lfe;
            this.platformInheritanceChain = platformInheritanceChain;
        }

        public string LibraryName { get { return library.Name; } }

        public void TranslateInvocation(
            StringBuilder sb,
            AbstractTranslator translator,
            string functionName,
            Expression[] args,
            Pastel.Token throwToken)
        {
            try
            {
                sb.Append(Crayon.LibraryExporter.Get(this.library, translator.Platform).TranslateNativeInvocation(throwToken, translator, functionName, args));
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
