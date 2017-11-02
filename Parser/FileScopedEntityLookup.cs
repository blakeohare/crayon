using Parser.ParseTree;
using System.Collections.Generic;
using System.Linq;

namespace Parser
{
    class FileScopedEntityLookup
    {
        private bool initialized = false;
        private FileScope fileScope;
        private Dictionary<string, object> depsLookup;
        private Dictionary<string, object> scopeLookup;

        private string[] importStatements;

        public FileScopedEntityLookup SetFileScope(FileScope fileScope)
        {
            this.fileScope = fileScope;
            return this;
        }

        // whittles down the universal lookup to just the things that were imported.
        public void InitializeLookups(
            Dictionary<string, TopLevelConstruct> depsEntityLookup,
            Dictionary<string, TopLevelConstruct> compilationScopeEntityLookup,
            Dictionary<string, NamespaceReferenceTemplate> depsNamespaceLookup,
            Dictionary<string, NamespaceReferenceTemplate> compilationScopeNamespaceLookup)
        {
            if (this.initialized) throw new System.Exception(); // This should not happen.
            this.initialized = true;

            // TODO: This is horrificly hacky and I don't like it but oh well.
            // Maybe add a common interface for these Dictionary values.
            Dictionary<string, object> depsLookupBuilder = new Dictionary<string, object>();
            this.scopeLookup = new Dictionary<string, object>();
            foreach (string key in depsEntityLookup.Keys) depsLookupBuilder[key] = depsEntityLookup[key];
            foreach (string key in depsNamespaceLookup.Keys) depsLookupBuilder[key] = depsNamespaceLookup[key];
            foreach (string key in compilationScopeEntityLookup.Keys) this.scopeLookup[key] = compilationScopeEntityLookup[key];
            foreach (string key in compilationScopeNamespaceLookup.Keys) this.scopeLookup[key] = compilationScopeNamespaceLookup[key];

            Dictionary<string, object> visibleItems = new Dictionary<string, object>();
            this.importStatements = this.fileScope.Imports.Select(t => t.ImportPath).ToArray();

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
                if (depsLookupBuilder.ContainsKey(topLevelNamespace))
                {
                    visibleItems[topLevelNamespace] = depsLookupBuilder[topLevelNamespace];
                }
            }

            foreach (string path in depsLookupBuilder.Keys)
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
                            visibleItems[path] = depsLookupBuilder[path];
                            break;
                        }
                    }
                }
            }

            this.depsLookup = visibleItems;
        }

        public NamespaceReferenceTemplate DoNamespaceLookup(string name, TopLevelConstruct currentEntity)
        {
            return this.DoLookupImpl(name, currentEntity) as NamespaceReferenceTemplate;
        }

        public TopLevelConstruct DoEntityLookup(string name, TopLevelConstruct currentEntity)
        {
            return this.DoLookupImpl(name, currentEntity) as TopLevelConstruct;
        }

        // Note: wraapping namespaces is a list of the namespace chains in a popped order...
        // namespace MyNamespace.Foo.Bar.Baz { ... } will result in...
        //   ["MyNamespace.Foo.Bar.Baz", "MyNamespace.Foo.Bar", "MyNamespace.Foo", "MyNamespace"]
        public object DoLookupImpl(
            string name,
            TopLevelConstruct currentEntity)
        {
            // check for that entity in the current compilation scope,
            if (scopeLookup.ContainsKey(name)) return scopeLookup[name];

            // check for that entity in another compilation scope
            if (depsLookup.ContainsKey(name)) return depsLookup[name];

            string[] wrappingNamespaces = currentEntity.LocalNamespace;

            // if there's a namespace or series of namespaces, check if it's in that fully-specified namespace
            if (wrappingNamespaces.Length > 0)
            {
                string fullyQualified = wrappingNamespaces[0] + "." + name;
                if (scopeLookup.ContainsKey(fullyQualified)) return scopeLookup[fullyQualified];
            }

            // Go through all the imports and check to see if any of them fully qualify it as a prefix.
            for (int i = 0; i < this.importStatements.Length; ++i)
            {
                string fullyQualified = this.importStatements[i] + "." + name;
                if (scopeLookup.ContainsKey(fullyQualified)) return scopeLookup[fullyQualified];
                if (depsLookup.ContainsKey(fullyQualified)) return depsLookup[fullyQualified];
            }

            // Now go back through the wrapping namespaces and check each fragment in decreasing order.
            for (int i = 1; i < wrappingNamespaces.Length; ++i)
            {
                string fullyQualified = wrappingNamespaces[i] + "." + name;
                if (scopeLookup.ContainsKey(fullyQualified)) return scopeLookup[fullyQualified];
            }

            return null;
        }
    }
}
