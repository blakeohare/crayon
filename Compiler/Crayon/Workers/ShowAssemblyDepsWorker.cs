using AssemblyResolver;
using Common;
using Parser;
using System.Linq;

namespace Crayon
{
    internal class ShowAssemblyDepsWorker
    {
        public void DoWorkImpl(CompilationScope scope)
        {
            AssemblyMetadata[] libraryMetadata = scope.Dependencies
                .Select(localizedLibView => localizedLibView.Scope.Metadata)
                .ToArray();

            string depTree = AssemblyDependencyResolver.GetDependencyTreeJson(libraryMetadata).Trim();
            ConsoleWriter.Print(ConsoleMessageType.LIBRARY_TREE, depTree);
        }
    }
}
