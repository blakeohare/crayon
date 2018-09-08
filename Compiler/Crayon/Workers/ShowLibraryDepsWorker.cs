using Parser;
using System;
using System.Linq;

namespace Crayon
{
    internal class ShowLibraryDepsWorker
    {
        public void DoWorkImpl(CompilationScope scope)
        {
            AssemblyMetadata[] libraryMetadata = scope
                .Dependencies
                .Select(libLocView => libLocView.LibraryScope.Metadata)
                .ToArray();

            string libs = LibraryDependencyResolver.GetDependencyTreeLog(libraryMetadata);
            Console.WriteLine("<LibraryDependencies>");
            Console.WriteLine(libs.Trim());
            Console.WriteLine("</LibraryDependencies>");
        }
    }
}
