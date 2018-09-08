using Parser;
using System;
using System.Linq;

namespace Crayon
{
    internal class ShowLibraryDepsWorker
    {
        public void DoWorkImpl(CompilationScope scope)
        {
            LibraryMetadata[] libraryMetadata = scope
                .Dependencies
                .Select(libLocView => libLocView.LibraryScope.Library)
                .ToArray();

            string libs = LibraryDependencyResolver.GetDependencyTreeLog(libraryMetadata);
            Console.WriteLine("<LibraryDependencies>");
            Console.WriteLine(libs.Trim());
            Console.WriteLine("</LibraryDependencies>");
        }
    }
}
