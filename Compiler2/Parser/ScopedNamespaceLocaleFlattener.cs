using Common;
using Localization;
using Parser.ParseTree;
using System.Collections.Generic;

namespace Parser
{
    internal class ScopedNamespaceLocaleFlattener
    {
        private NamespaceTreeNode root = new NamespaceTreeNode("*", null, -1);

        private Dictionary<Locale, Dictionary<string, NamespaceReferenceTemplate>> finalizedLookupForLocales =
            new Dictionary<Locale, Dictionary<string, NamespaceReferenceTemplate>>();

        private class NamespaceTreeNode
        {
            public NamespaceTreeNode(string name, NamespaceTreeNode parent, int offsetFromRoot)
            {
                this.LeafName = name;
                this.Parent = parent;
                this.OriginalDefinitions = new List<Namespace>();
                this.Children = new Dictionary<string, NamespaceTreeNode>();
                this.NamesByLocale = new Dictionary<Locale, string>();
                this.OffsetFromRoot = offsetFromRoot;
            }
            public string LeafName { get; private set; }

            public int OffsetFromRoot { get; private set; }

            // For example, namespace A { namespace B { ...  } }, namespace B would have an offset of 1
            public List<Namespace> OriginalDefinitions { get; private set; }

            public Dictionary<Locale, string> NamesByLocale { get; private set; }

            public NamespaceTreeNode Parent { get; private set; }
            public Dictionary<string, NamespaceTreeNode> Children { get; private set; }
            public void AddNamespace(Namespace ns, Queue<string> nsParts)
            {
                string segment = nsParts.Dequeue();
                if (segment != this.LeafName) throw new System.Exception(); // this shouldn't happen

                if (ns.NestDepth <= this.OffsetFromRoot)
                {
                    this.OriginalDefinitions.Add(ns);
                }
                if (nsParts.Count > 0)
                {
                    string nextKey = nsParts.Peek();
                    if (!this.Children.ContainsKey(nextKey))
                    {
                        this.Children.Add(nextKey, new NamespaceTreeNode(nextKey, this, this.OffsetFromRoot + 1));
                    }
                    this.Children[nextKey].AddNamespace(ns, nsParts);
                }
            }
        }

        internal void AddNamespace(Namespace ns)
        {
            Queue<string> segments = new Queue<string>();
            segments.Enqueue("*");
            foreach (string segment in ns.FullyQualifiedDefaultNameSegments)
            {
                segments.Enqueue(segment);
            }

            this.root.AddNamespace(ns, segments);
        }

        public Dictionary<string, NamespaceReferenceTemplate> GetLookup(Locale locale)
        {
            if (!this.finalizedLookupForLocales.ContainsKey(locale))
            {
                this.finalizedLookupForLocales[locale] = this.GenerateLookupForLocale(locale);
            }
            return this.finalizedLookupForLocales[locale];
        }

        private Dictionary<string, NamespaceReferenceTemplate> GenerateLookupForLocale(Locale locale)
        {
            Dictionary<string, NamespaceReferenceTemplate> lookup = new Dictionary<string, NamespaceReferenceTemplate>();
            this.GenerateLookupForLocaleImpl(lookup, locale, this.root, null);
            return lookup;
        }

        private void GenerateLookupForLocaleImpl(
            Dictionary<string, NamespaceReferenceTemplate> lookup,
            Locale locale,
            NamespaceTreeNode node,
            string namespaceBuilder)
        {
            string localizedName = null;
            Namespace representative = null;
            foreach (Namespace ns in node.OriginalDefinitions)
            {
                if (ns.NamesByLocale.ContainsKey(locale))
                {
                    string newLocalizedName = ns.NamesByLocale[locale][node.OffsetFromRoot - ns.NestDepth];

                    if (localizedName != null && newLocalizedName != localizedName)
                    {
                        TODO.ThisErrorMessageIsNotVeryHelpeful();
                        throw new ParserException(ns, "This namespace definition has a different localized name somewhere else.");
                    }
                    localizedName = newLocalizedName;
                    representative = ns;
                }
            }

            if (node.LeafName != "*")
            {
                if (localizedName == null)
                {
                    localizedName = node.LeafName;
                }
                namespaceBuilder = namespaceBuilder == null ? localizedName : (namespaceBuilder + "." + localizedName);
            }

            if (namespaceBuilder != null)
            {
                lookup[namespaceBuilder] = new NamespaceReferenceTemplate()
                {
                    Name = namespaceBuilder,
                    OriginalNamespace = representative ?? node.OriginalDefinitions[0],
                    OriginalNamespaceDepthClipping = namespaceBuilder.Split('.').Length,
                };
            }

            foreach (NamespaceTreeNode child in node.Children.Values)
            {
                this.GenerateLookupForLocaleImpl(lookup, locale, child, namespaceBuilder);
            }
        }
    }
}
