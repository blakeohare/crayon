using Parser;
using System;
using System.Linq;

namespace Crayon
{
    internal class ShowAssemblyDepsWorker
    {
        public void DoWorkImpl(CompilationScope scope)
        {
            AssemblyMetadata[] libraryMetadata = scope
                .Dependencies
                .Select(libLocView => libLocView.LibraryScope.Metadata)
                .ToArray();

            string libs = AssemblyDependencyResolver.GetDependencyTreeLog(libraryMetadata);
            Console.WriteLine("<LibraryDependencies>");
            Console.WriteLine(libs.Trim());
            Console.WriteLine("</LibraryDependencies>");
        }
    }
}
