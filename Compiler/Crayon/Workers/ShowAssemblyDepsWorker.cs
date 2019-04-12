using Parser;
using System;
using System.Linq;

namespace Crayon
{
    internal class ShowAssemblyDepsWorker
    {
        public void DoWorkImpl(bool useOutputPrefix, CompilationScope scope)
        {
            AssemblyMetadata[] libraryMetadata = scope
                .Dependencies
                .Select(localizedLibView => localizedLibView.Scope.Metadata)
                .ToArray();

            string depTree = AssemblyDependencyResolver.GetDependencyTreeJson(libraryMetadata).Trim();
            if (!useOutputPrefix)
            {
                Console.WriteLine(depTree);
            }
            else
            {
                foreach (string line in depTree.Split('\n'))
                {
                    Console.WriteLine("LIBS: " + line.TrimEnd());
                }
            }
        }
    }
}
