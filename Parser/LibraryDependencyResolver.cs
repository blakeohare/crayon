using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parser
{
    public class LibraryDependencyResolver
    {
        // Essentially, just a post-order traversal
        public static LibraryCompilationScope[] GetLibraryResolutionOrder(ParserContext parser)
        {
            // these are alphabetized simply to guarantee consistent behavior.
            LibraryCompilationScope[] unorderedLibraries = parser.LibraryManager.ImportedLibraries.OrderBy(scope => scope.Library.ID.ToLowerInvariant()).ToArray();

            List<LibraryCompilationScope> orderedLibraries = new List<LibraryCompilationScope>();
            HashSet<LibraryCompilationScope> usedLibraries = new HashSet<LibraryCompilationScope>();

            HashSet<LibraryCompilationScope> cycleCheck = new HashSet<LibraryCompilationScope>();
            Stack<LibraryCompilationScope> breadcrumbs = new Stack<LibraryCompilationScope>();
            foreach (LibraryCompilationScope library in unorderedLibraries)
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
            LibraryCompilationScope libraryToUse,
            List<LibraryCompilationScope> libraryOrderOut,
            HashSet<LibraryCompilationScope> usedLibraries,
            HashSet<LibraryCompilationScope> cycleCheck,
            Stack<LibraryCompilationScope> breadcrumbs)
        {
            if (usedLibraries.Contains(libraryToUse)) return;

            breadcrumbs.Push(libraryToUse);

            if (cycleCheck.Contains(libraryToUse))
            {
                StringBuilder message = new StringBuilder();
                message.Append("There is a dependency cycle in your libraries: ");
                bool first = true;
                foreach (LibraryCompilationScope breadcrumb in breadcrumbs)
                {
                    if (first) first = false;
                    else message.Append(" -> ");
                    message.Append(breadcrumb.Library.ID);
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

        public static string GetDependencyTreeLog(LibraryMetadata[] libraries)
        {
            StringBuilder sb = new StringBuilder();
            foreach (LibraryMetadata library in libraries)
            {
                sb.Append(library.ID);
                sb.Append(": ");

                sb.Append(string.Join(" ",
                    new HashSet<string>(library.LibraryScope.Dependencies
                        .Select(lib => lib.LibraryScope.Library.ID))
                        .OrderBy(name => name.ToLower())));

                sb.Append("\n");
            }
            return sb.ToString();
        }
    }
}
