using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pastel.Nodes;
using Platform;

namespace Crayon
{
    class LibraryNativeInvocationTranslator : ILibraryNativeInvocationTranslator
    {
        private Library library;
        private IList<string> platformInheritanceChain;

        public LibraryNativeInvocationTranslator(Library library, IList<string> platformInheritanceChain)
        {
            this.library = library;
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
            sb.Append(this.library.TranslateNativeInvocation(throwToken, translator, functionName, args));
        }
    }
}
