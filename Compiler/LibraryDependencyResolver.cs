using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon
{
    class LibraryDependencyResolver
    {
        // Essentially, just a post-order traversal
        public static Library[] GetLibraryResolutionOrder(Parser parser)
        {
            // these are alphabetized simply to guarantee consistent behavior.
            Library[] unorderedLibraries = parser.LibraryManager.LibrariesUsed.OrderBy(lib => lib.Name.ToLowerInvariant()).ToArray();

            List<Library> orderedLibraries = new List<Library>();
            HashSet<Library> usedLibraries = new HashSet<Library>();

            HashSet<Library> cycleCheck = new HashSet<Library>();
            Stack<Library> breadcrumbs = new Stack<Library>();
            foreach (Library library in unorderedLibraries)
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
            Library libraryToUse,
            List<Library> libraryOrderOut,
            HashSet<Library> usedLibraries,
            HashSet<Library> cycleCheck,
            Stack<Library> breadcrumbs)
        {
            if (usedLibraries.Contains(libraryToUse)) return;

            breadcrumbs.Push(libraryToUse);

            if (cycleCheck.Contains(libraryToUse))
            {
                StringBuilder message = new StringBuilder();
                message.Append("There is a dependency cycle in your libraries: ");
                bool first = true;
                foreach (Library breadcrumb in breadcrumbs)
                {
                    if (first) first = false;
                    else message.Append(" -> ");
                    message.Append(breadcrumb.Name);
                }
                throw new InvalidOperationException(message.ToString());
            }
            cycleCheck.Add(libraryToUse);

            foreach (Library dependency in libraryToUse.LibraryDependencies)
            {
                LibraryUsagePostOrderTraversal(dependency, libraryOrderOut, usedLibraries, cycleCheck, breadcrumbs);
            }
            cycleCheck.Remove(libraryToUse);
            breadcrumbs.Pop();

            usedLibraries.Add(libraryToUse);
            libraryOrderOut.Add(libraryToUse);
        }
    }
}
