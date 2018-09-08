﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parser
{
    public class AssemblyDependencyResolver
    {
        // Essentially, just a post-order traversal
        public static CompilationScope[] GetAssemblyResolutionOrder(ParserContext parser)
        {
            // these are alphabetized simply to guarantee consistent behavior.
            CompilationScope[] unorderedScopes = parser.AssemblyManager.ImportedAssemblyScopes.OrderBy(scope => scope.Metadata.ID.ToLowerInvariant()).ToArray();

            List<CompilationScope> orderedLibraries = new List<CompilationScope>();
            HashSet<CompilationScope> usedLibraries = new HashSet<CompilationScope>();

            HashSet<CompilationScope> cycleCheck = new HashSet<CompilationScope>();
            Stack<CompilationScope> breadcrumbs = new Stack<CompilationScope>();
            foreach (CompilationScope compilationScope in unorderedScopes)
            {
                if (!usedLibraries.Contains(compilationScope))
                {
                    LibraryUsagePostOrderTraversal(
                        compilationScope, orderedLibraries, usedLibraries, cycleCheck, breadcrumbs);
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

            foreach (LocalizedAssemblyView dependency in libraryToUse.Dependencies)
            {
                LibraryUsagePostOrderTraversal(dependency.Scope, libraryOrderOut, usedLibraries, cycleCheck, breadcrumbs);
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
                        .Select(lib => lib.Scope.Metadata.ID))
                        .OrderBy(name => name.ToLower())));

                sb.Append("\n");
            }
            return sb.ToString();
        }
    }
}
