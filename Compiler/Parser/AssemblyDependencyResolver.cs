using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parser
{
    public class AssemblyDependencyResolver
    {
        // Essentially, just a post-order traversal
        public static AssemblyMetadata[] GetAssemblyResolutionOrder(IEnumerable<AssemblyMetadata> usedAssemblies)
        {
            // these are alphabetized simply to guarantee consistent behavior.
            AssemblyMetadata[] unorderedAssemblies = usedAssemblies.OrderBy(md => md.ID.ToLowerInvariant()).ToArray();

            List<AssemblyMetadata> orderedLibraries = new List<AssemblyMetadata>();
            HashSet<AssemblyMetadata> usedLibraries = new HashSet<AssemblyMetadata>();

            HashSet<AssemblyMetadata> cycleCheck = new HashSet<AssemblyMetadata>();
            Stack<AssemblyMetadata> breadcrumbs = new Stack<AssemblyMetadata>();
            foreach (AssemblyMetadata assembly in unorderedAssemblies)
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
            AssemblyMetadata libraryToUse,
            List<AssemblyMetadata> libraryOrderOut,
            HashSet<AssemblyMetadata> usedLibraries,
            HashSet<AssemblyMetadata> cycleCheck,
            Stack<AssemblyMetadata> breadcrumbs)
        {
            if (usedLibraries.Contains(libraryToUse)) return;

            breadcrumbs.Push(libraryToUse);

            if (cycleCheck.Contains(libraryToUse))
            {
                StringBuilder message = new StringBuilder();
                message.Append("There is a dependency cycle in your libraries: ");
                bool first = true;
                foreach (AssemblyMetadata breadcrumb in breadcrumbs)
                {
                    if (first) first = false;
                    else message.Append(" -> ");
                    message.Append(breadcrumb.ID);
                }
                throw new InvalidOperationException(message.ToString());
            }
            cycleCheck.Add(libraryToUse);

            foreach (AssemblyMetadata dependency in libraryToUse.DirectDependencies)
            {
                LibraryUsagePostOrderTraversal(dependency, libraryOrderOut, usedLibraries, cycleCheck, breadcrumbs);
            }
            cycleCheck.Remove(libraryToUse);
            breadcrumbs.Pop();

            usedLibraries.Add(libraryToUse);
            libraryOrderOut.Add(libraryToUse);
        }

        private static void LibraryDepTreeFlattenerRecursive(
            Dictionary<string, AssemblyMetadata> libsOut,
            AssemblyMetadata current)
        {
            string id = current.ID;
            if (!libsOut.ContainsKey(id))
            {
                libsOut[id] = current;
                foreach (AssemblyMetadata dep in current.DirectDependencies)
                {
                    LibraryDepTreeFlattenerRecursive(libsOut, dep);
                }
            }
        }

        private static AssemblyMetadata[] LibraryDepTreeFlattener(AssemblyMetadata[] topLevelDeps)
        {
            Dictionary<string, AssemblyMetadata> flattened = new Dictionary<string, AssemblyMetadata>();
            foreach (AssemblyMetadata dep in topLevelDeps)
            {
                LibraryDepTreeFlattenerRecursive(flattened, dep);
            }
            List<AssemblyMetadata> output = new List<AssemblyMetadata>();
            foreach (string flattenedKey in flattened.Keys.OrderBy(n => n.ToLower()))
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
        public static string GetDependencyTreeJson(AssemblyMetadata[] libraries)
        {
            Dictionary<string, string[]> depsById = new Dictionary<string, string[]>();
            string[] rootDeps = libraries.Select(am => am.ID).OrderBy(n => n.ToLower()).ToArray();

            Dictionary<string, AssemblyMetadata> allLibraries = new Dictionary<string, AssemblyMetadata>();

            foreach (AssemblyMetadata lib in LibraryDepTreeFlattener(libraries))
            {
                string[] deps = new HashSet<string>(lib.DirectDependencies
                    .Select(assembly => assembly.ID))
                    .OrderBy(name => name.ToLower()).ToArray();

                depsById[lib.ID] = deps;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("{\n  \"rootDeps\": [");
            for (int i = 0; i < rootDeps.Length; ++i)
            {
                if (i > 0) sb.Append(", ");
                sb.Append('"');
                sb.Append(rootDeps[i]);
                sb.Append('"');
            }
            sb.Append("],\n  \"allLibScopes\": [\n");
            string[] ids = depsById.Keys.OrderBy(v => v.ToLower()).ToArray();
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
