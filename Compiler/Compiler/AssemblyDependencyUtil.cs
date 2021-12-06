using System;
using System.Collections.Generic;
using System.Linq;

namespace Builder
{
    internal static class AssemblyDependencyUtil
    {
        // Essentially, just a post-order traversal
        public static ExternalAssemblyMetadata[] GetAssemblyResolutionOrder(IEnumerable<ExternalAssemblyMetadata> usedAssemblies)
        {
            // these are alphabetized simply to guarantee consistent behavior.
            ExternalAssemblyMetadata[] unorderedAssemblies = usedAssemblies.OrderBy(md => md.ID.ToLowerInvariant()).ToArray();

            List<ExternalAssemblyMetadata> orderedLibraries = new List<ExternalAssemblyMetadata>();
            HashSet<ExternalAssemblyMetadata> usedLibraries = new HashSet<ExternalAssemblyMetadata>();

            HashSet<ExternalAssemblyMetadata> cycleCheck = new HashSet<ExternalAssemblyMetadata>();
            Stack<ExternalAssemblyMetadata> breadcrumbs = new Stack<ExternalAssemblyMetadata>();
            foreach (ExternalAssemblyMetadata assembly in unorderedAssemblies)
            {
                if (!usedLibraries.Contains(assembly))
                {
                    LibraryUsagePostOrderTraversal(
                        assembly, orderedLibraries, usedLibraries, cycleCheck, breadcrumbs);
                    cycleCheck.Clear();
                    breadcrumbs.Clear();
                }
            }

            return orderedLibraries.ToArray();
        }

        private static void LibraryUsagePostOrderTraversal(
            ExternalAssemblyMetadata libraryToUse,
            List<ExternalAssemblyMetadata> libraryOrderOut,
            HashSet<ExternalAssemblyMetadata> usedLibraries,
            HashSet<ExternalAssemblyMetadata> cycleCheck,
            Stack<ExternalAssemblyMetadata> breadcrumbs)
        {
            if (usedLibraries.Contains(libraryToUse)) return;

            breadcrumbs.Push(libraryToUse);

            if (cycleCheck.Contains(libraryToUse))
            {
                System.Text.StringBuilder message = new System.Text.StringBuilder();
                message.Append("There is a dependency cycle in your libraries: ");
                bool first = true;
                foreach (ExternalAssemblyMetadata breadcrumb in breadcrumbs)
                {
                    if (first) first = false;
                    else message.Append(" -> ");
                    message.Append(breadcrumb.ID);
                }
                throw new InvalidOperationException(message.ToString());
            }
            cycleCheck.Add(libraryToUse);

            foreach (ExternalAssemblyMetadata dependency in libraryToUse.DirectDependencies)
            {
                LibraryUsagePostOrderTraversal(dependency, libraryOrderOut, usedLibraries, cycleCheck, breadcrumbs);
            }
            cycleCheck.Remove(libraryToUse);
            breadcrumbs.Pop();

            usedLibraries.Add(libraryToUse);
            libraryOrderOut.Add(libraryToUse);
        }

        private static void LibraryDepTreeFlattenerRecursive(
            Dictionary<string, ExternalAssemblyMetadata> libsOut,
            ExternalAssemblyMetadata current)
        {
            string id = current.ID;
            if (!libsOut.ContainsKey(id))
            {
                libsOut[id] = current;
                foreach (ExternalAssemblyMetadata dep in current.DirectDependencies)
                {
                    LibraryDepTreeFlattenerRecursive(libsOut, dep);
                }
            }
        }

        private static ExternalAssemblyMetadata[] LibraryDepTreeFlattener(ExternalAssemblyMetadata[] topLevelDeps)
        {
            Dictionary<string, ExternalAssemblyMetadata> flattened = new Dictionary<string, ExternalAssemblyMetadata>();
            foreach (ExternalAssemblyMetadata dep in topLevelDeps)
            {
                LibraryDepTreeFlattenerRecursive(flattened, dep);
            }
            List<ExternalAssemblyMetadata> output = new List<ExternalAssemblyMetadata>();
            foreach (string flattenedKey in flattened.Keys.OrderBy(n => n.ToLowerInvariant()))
            {
                output.Add(flattened[flattenedKey]);
            }
            return output.ToArray();
        }

        /*
            returns a JSON string like this:
            {
                "rootDeps": [ "Math", "Core", "Game", "Graphics2D", "UserData" ],
                "allLibScopes": [
                  { "name": "Graphics2D", "deps": [ "Core", "Math", "Game" ] },
                  { "name": "UserData", "deps": [ "Core", "FileIOCommon" ] },
                  { "name": "FileIOCommon", "deps": [ "Core" ] },
                  etc...
                ]
            }

            The "allLibScopes" field contains a list of ALL library dependencies, including
            deep dependencies. "rootDeps" is the "name" value of just the library dependencies
            used by the root user-defined scope.
        */
        public static string GetDependencyTreeJson(ExternalAssemblyMetadata[] libraries)
        {
            Dictionary<string, string[]> depsById = new Dictionary<string, string[]>();
            string[] rootDeps = libraries.Select(am => am.ID).OrderBy(n => n.ToLowerInvariant()).ToArray();

            foreach (ExternalAssemblyMetadata lib in LibraryDepTreeFlattener(libraries))
            {
                string[] deps = new HashSet<string>(lib.DirectDependencies
                    .Select(assembly => assembly.ID))
                    .OrderBy(name => name.ToLowerInvariant()).ToArray();

                depsById[lib.ID] = deps;
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("{\n  \"rootDeps\": [");
            for (int i = 0; i < rootDeps.Length; ++i)
            {
                if (i > 0) sb.Append(", ");
                sb.Append('"');
                sb.Append(rootDeps[i]);
                sb.Append('"');
            }
            sb.Append("],\n  \"allLibScopes\": [\n");
            string[] ids = depsById.Keys.OrderBy(v => v.ToLowerInvariant()).ToArray();
            for (int i = 0; i < ids.Length; ++i)
            {
                sb.Append("    { \"name\": \"");
                sb.Append(ids[i]);
                sb.Append("\", \"deps\": [");
                string[] depDeps = depsById[ids[i]];
                for (int j = 0; j < depDeps.Length; ++j)
                {
                    if (j > 0) sb.Append(", ");
                    sb.Append('"');
                    sb.Append(depDeps[j]);
                    sb.Append('"');
                }
                sb.Append("] }");
                if (i < ids.Length - 1)
                {
                    sb.Append(',');
                }
                sb.Append("\n");
            }
            sb.Append("  ]\n}");
            return sb.ToString();
        }
    }
}
