using Crayon.ParseTree;
using System.Collections.Generic;
using System.Linq;

namespace Crayon
{
    class FileScopedEntityLookup
    {
        private FileScope fileScope;
        private Dictionary<string, TopLevelConstruct> libraryEntities;
        private Dictionary<string, TopLevelConstruct> localEntities;
        private string[] importStatements;

        public FileScopedEntityLookup SetFileScope(FileScope fileScope)
        {
            this.fileScope = fileScope;
            return this;
        }

        // whittles down the universal lookup to just the things that were imported.
        public void InitializeLookups(
            Dictionary<string, TopLevelConstruct> universalLookup,
            Dictionary<string, TopLevelConstruct> compilationScopeLookup)
        {
            if (this.importStatements != null) return;

            Dictionary<string, TopLevelConstruct> visibleItems = new Dictionary<string, TopLevelConstruct>();
            this.importStatements = this.fileScope.Imports.Select(t => t.ImportPath).ToArray();
            this.localEntities = compilationScopeLookup;

            // Since this lookup would otherwise be O(number of imports * number of universal entities),
            // optimize this by creating a lookup based on the first character of the import. Generally this will be unique
            // which brings the effective complexity down to O(number of universal entities)
            Dictionary<char, HashSet<string>> byCharLookup = new Dictionary<char, HashSet<string>>();
            char firstChar;
            foreach (string importPath in this.importStatements)
            {
                firstChar = importPath[0];
                if (!byCharLookup.ContainsKey(firstChar))
                {
                    byCharLookup[firstChar] = new HashSet<string>();
                }
                byCharLookup[firstChar].Add(importPath + ".");

                string topLevelNamespace = importPath.Split('.')[0];
                if (universalLookup.ContainsKey(topLevelNamespace))
                {
                    visibleItems[topLevelNamespace] = universalLookup[topLevelNamespace];
                }
            }

            foreach (string path in universalLookup.Keys)
            {
                firstChar = path[0];
                if (byCharLookup.ContainsKey(firstChar))
                {
                    // the optimization is in the hope that this loop will run typically 0 or 1 times.
                    // It'd be super-optimized if this approach was done instead to the universal lookup, but
                    // the universal lookup should be treated as immutable.
                    // TODO: create a UniversalLookup class that is a tiered dictionary where the first level
                    // has the first component of the fully-qualified namespace. Then this becomes trivial and fast.
                    foreach (string importedPaths in byCharLookup[firstChar])
                    {
                        if (path.StartsWith(importedPaths))
                        {
                            visibleItems[path] = universalLookup[path];
                            break;
                        }
                    }
                }
            }

            this.libraryEntities = visibleItems;
        }

        // Note: wraapping namespaces is a list of the namespace chains in a popped order...
        // namespace MyNamespace.Foo.Bar.Baz { ... } will result in...
        //   ["MyNamespace.Foo.Bar.Baz", "MyNamespace.Foo.Bar", "MyNamespace.Foo", "MyNamespace"]
        public TopLevelConstruct DoLookup(string name, TopLevelConstruct currentEntity)
        {
            // check for that entity in the current compilation scope,
            if (this.localEntities.ContainsKey(name)) return this.localEntities[name];

            // check for that entity in another compilation scope
            if (this.libraryEntities.ContainsKey(name)) return this.libraryEntities[name];

            string[] wrappingNamespaces = currentEntity.LocalNamespace;

            // if there's a namespace or series of namespaces, check if it's in that fully-specified namespace
            if (wrappingNamespaces.Length > 0)
            {
                string fullyQualified = wrappingNamespaces[0] + "." + name;
                if (this.localEntities.ContainsKey(fullyQualified)) return this.localEntities[fullyQualified];
            }

            // Go through all the imports and check to see if any of them fully qualify it as a prefix.
            for (int i = 0; i < this.importStatements.Length; ++i)
            {
                string fullyQualified = this.importStatements[i] + "." + name;
                if (this.localEntities.ContainsKey(fullyQualified)) return this.localEntities[fullyQualified];
                if (this.libraryEntities.ContainsKey(fullyQualified)) return this.libraryEntities[fullyQualified];
            }

            // Now go back through the wrapping namespaces and check each fragment in decreasing order.
            for (int i = 1; i < wrappingNamespaces.Length; ++i)
            {
                string fullyQualified = wrappingNamespaces[i] + "." + name;
                if (this.localEntities.ContainsKey(fullyQualified)) return this.localEntities[fullyQualified];
            }

            return null;
        }
    }
}
