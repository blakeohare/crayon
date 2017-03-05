using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform
{
    public interface ILibraryNativeInvocationTranslatorProvider
    {
        ILibraryNativeInvocationTranslator GetTranslator(
            
            // Name of the library
            string libraryName);
    }
}
