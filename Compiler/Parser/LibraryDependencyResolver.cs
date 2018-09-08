using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parser
{
    public class LibraryDependencyResolver
    {
        // Essentially, just a post-order traversal
        public static CompilationScope[] GetLibraryResolutionOrder(ParserContext parser)
        {
            // these are alphabetized simply to guarantee consistent behavior.
            CompilationScope[] unorderedLibraries = parser.LibraryManager.ImportedLibraries.OrderBy(scope => scope.Metadata.ID.ToLowerInvariant()).ToArray();

            List<CompilationScope> orderedLibraries = new List<CompilationScope>();
            HashSet<CompilationScope> usedLibraries = new HashSet<CompilationScope>();

            HashSet<CompilationScope> cycleCheck = new HashSet<CompilationScope>();
            Stack<CompilationScope> breadcrumbs = new Stack<CompilationScope>();
            foreach (CompilationScope library in unorderedLibraries)
            {
                if (!usedLibraries.Contains(library))
                {
                    LibraryUsagePostOrderTraversal(
                        library, orderedLibraries, usedLibraries, cycleCheck, breadcrumbs);
                    cycleCheck.Clear();
                    breadcrumbs.Clear();
                }
            }

            return orderedLibraries.ToArray();
        }

        private static void LibraryUsagePostOrderTraversal(
            CompilationScope libraryToUse,
            List<CompilationScope> libraryOrderOut,
            HashSet<CompilationScope> usedLibraries,
            HashSet<CompilationScope> cycleCheck,
            Stack<CompilationScope> breadcrumbs)
        {
            if (usedLibraries.Contains(libraryToUse)) return;

            breadcrumbs.Push(libraryToUse);

            if (cycleCheck.Contains(libraryToUse))
            {
                StringBuilder message = new StringBuilder();
                message.Append("There is a dependency cycle in your libraries: ");
                bool first = true;
                foreach (CompilationScope breadcrumb in breadcrumbs)
                {
                    if (first) first = false;
                    else message.Append(" -> ");
                    message.Append(breadcrumb.Metadata.ID);
                }
                throw new InvalidOperationException(message.ToString());
            }
            cycleCheck.Add(libraryToUse);

            foreach (LocalizedLibraryView dependency in libraryToUse.Dependencies)
            {
                LibraryUsagePostOrderTraversal(dependency.LibraryScope, libraryOrderOut, usedLibraries, cycleCheck, breadcrumbs);
            }
            cycleCheck.Remove(libraryToUse);
            breadcrumbs.Pop();

            usedLibraries.Add(libraryToUse);
            libraryOrderOut.Add(libraryToUse);
        }

        public static string GetDependencyTreeLog(AssemblyMetadata[] libraries)
        {
            StringBuilder sb = new StringBuilder();
            foreach (AssemblyMetadata library in libraries)
            {
                sb.Append(library.ID);
                sb.Append(": ");

                sb.Append(string.Join(" ",
                    new HashSet<string>(library.Scope.Dependencies
                        .Select(lib => lib.LibraryScope.Metadata.ID))
                        .OrderBy(name => name.ToLower())));

                sb.Append("\n");
            }
            return sb.ToString();
        }
    }
}
