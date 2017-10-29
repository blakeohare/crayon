using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon
{
    class LibraryDependencyResolver
    {
        // Essentially, just a post-order traversal
        public static LibraryMetadata[] GetLibraryResolutionOrder(ParserContext parser)
        {
            // these are alphabetized simply to guarantee consistent behavior.
            LibraryMetadata[] unorderedLibraries = parser.LibraryManager.LibrariesUsed.OrderBy(lib => lib.Name.ToLowerInvariant()).Select(lib => lib.Metadata).ToArray();

            List<LibraryMetadata> orderedLibraries = new List<LibraryMetadata>();
            HashSet<LibraryMetadata> usedLibraries = new HashSet<LibraryMetadata>();

            HashSet<LibraryMetadata> cycleCheck = new HashSet<LibraryMetadata>();
            Stack<LibraryMetadata> breadcrumbs = new Stack<LibraryMetadata>();
            foreach (LibraryMetadata library in unorderedLibraries)
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
            LibraryMetadata libraryToUse,
            List<LibraryMetadata> libraryOrderOut,
            HashSet<LibraryMetadata> usedLibraries,
            HashSet<LibraryMetadata> cycleCheck,
            Stack<LibraryMetadata> breadcrumbs)
        {
            if (usedLibraries.Contains(libraryToUse)) return;

            breadcrumbs.Push(libraryToUse);

            if (cycleCheck.Contains(libraryToUse))
            {
                StringBuilder message = new StringBuilder();
                message.Append("There is a dependency cycle in your libraries: ");
                bool first = true;
                foreach (LibraryMetadata breadcrumb in breadcrumbs)
                {
                    if (first) first = false;
                    else message.Append(" -> ");
                    message.Append(breadcrumb.Name);
                }
                throw new InvalidOperationException(message.ToString());
            }
            cycleCheck.Add(libraryToUse);

            foreach (LibraryMetadata dependency in libraryToUse.LibraryDependencies)
            {
                LibraryUsagePostOrderTraversal(dependency, libraryOrderOut, usedLibraries, cycleCheck, breadcrumbs);
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
                sb.Append(library.Name);
                sb.Append(": ");

                sb.Append(string.Join(" ",
                    new HashSet<string>(library.LibraryDependencies.Select(lib => lib.Name))
                        .OrderBy(name => name.ToLower())));

                sb.Append("\n");
            }
            return sb.ToString();
        }
    }
}
