using System;
using System.Collections.Generic;
using Platform;

namespace Crayon
{
    class LibraryNativeInvocationTranslatorProvider : ILibraryNativeInvocationTranslatorProvider
    {
        private Dictionary<string, Library> libraries;
        private List<LibraryForExport> librariesForExport;
        private AbstractPlatform platform;

        public LibraryNativeInvocationTranslatorProvider(
            Dictionary<string, Library> libraries,
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

            return new LibraryNativeInvocationTranslator(this.libraries[libraryName], libraryForExport, this.platform.InheritanceChain);
        }
    }
}
