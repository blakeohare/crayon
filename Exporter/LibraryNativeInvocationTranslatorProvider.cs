using Parser;
using Platform;
using System;
using System.Collections.Generic;

namespace Crayon
{
    class LibraryNativeInvocationTranslatorProvider : ILibraryNativeInvocationTranslatorProvider
    {
        private Dictionary<string, LibraryMetadata> libraries;
        private List<LibraryForExport> librariesForExport;
        private AbstractPlatform platform;

        public LibraryNativeInvocationTranslatorProvider(
            Dictionary<string, LibraryMetadata> libraries,
            List<LibraryForExport> librariesForExport,
            AbstractPlatform platform)
        {
            this.libraries = libraries;
            this.librariesForExport = librariesForExport;
            this.platform = platform;
        }

        public ILibraryNativeInvocationTranslator GetTranslator(string libraryName)
        {
            LibraryForExport libraryForExport = null;
            foreach (LibraryForExport lfe in this.librariesForExport)
            {
                if (lfe.Name == libraryName)
                {
                    libraryForExport = lfe;
                    break;
                }
            }

            if (libraryForExport == null)
            {
                throw new Exception();
            }

            return new LibraryNativeInvocationTranslator(this.libraries[libraryName], libraryForExport);
        }
    }
}
