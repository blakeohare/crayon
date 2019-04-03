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

            Console.WriteLine(AssemblyDependencyResolver.GetDependencyTreeJson(libraryMetadata));
        }
    }
}
