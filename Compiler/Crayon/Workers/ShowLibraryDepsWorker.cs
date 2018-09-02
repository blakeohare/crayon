using Exporter;
using Parser;
using System;
using System.Linq;

namespace Crayon
{
    internal class ShowLibraryDepsWorker
    {
        public void DoWorkImpl(CompilationBundle compilationResult)
        {
            LibraryMetadata[] libraryMetadata = compilationResult
                .UserCodeScope
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
