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
                .Select(assemblyView => assemblyView.Scope.Metadata)
                .ToArray();

            string libs = AssemblyDependencyResolver.GetDependencyTreeLog(libraryMetadata);
            // TODO: if you change this, you need to update the TwoCans compiler service.
            Console.WriteLine("<LibraryDependencies>");
            Console.WriteLine(libs.Trim());
            Console.WriteLine("</LibraryDependencies>");
        }
    }
}
