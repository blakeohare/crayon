using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform;

namespace Crayon
{
    class LibraryNativeInvocationTranslatorProvider : ILibraryNativeInvocationTranslatorProvider
    {
        private Dictionary<string, Library> libraries;
        private Platform.AbstractPlatform platform;

        public LibraryNativeInvocationTranslatorProvider(Dictionary<string, Library> libraries, Platform.AbstractPlatform platform)
        {
            this.libraries = libraries;
            this.platform = platform;
        }

        public ILibraryNativeInvocationTranslator GetTranslator(string libraryName)
        {
            return new LibraryNativeInvocationTranslator(this.libraries[libraryName], this.platform.InheritanceChain);
        }
    }
}
