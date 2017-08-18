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
            try
            {
                sb.Append(this.library.TranslateNativeInvocation(throwToken, translator, functionName, args));
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
