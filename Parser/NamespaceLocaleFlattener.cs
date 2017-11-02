using Common;
using Localization;
using Parser.ParseTree;
using System.Collections.Generic;
using System.Linq;

namespace Parser
{
    internal static class NamespaceLocaleFlattener
    {
        public static Dictionary<string, NamespaceReferenceTemplate> GetLookupInDefaultLocale(Multimap<string, Namespace> namespaces)
        {
            Dictionary<string, NamespaceReferenceTemplate> output = new Dictionary<string, NamespaceReferenceTemplate>();
            foreach (string key in namespaces.Keys.OrderBy(k => k))
            {
                foreach (Namespace ns in namespaces[key])
                {
                    output[key] = new NamespaceReferenceTemplate() { Name = key, OriginalNamespace = ns };
                    break;
                }
            }
            return output;
        }

        public static Dictionary<string, NamespaceReferenceTemplate> GetLookup(
            Multimap<string, Namespace> namespaces,
            Locale locale)
        {
            // TODO: make a library that has "namespace A.B { ... }" and nothing else outside of that.
            TODO.UnitTestNeeded.LibraryScopeWithOnlyNestedNamespaces();

            // Try to annotate multiple identical namespaces with different localization annotations.
            TODO.UnitTestNeeded.LibraryWithInconsistentLocalizationAnnotations();

            Dictionary<string, NamespaceDescriptor> builder = new Dictionary<string, NamespaceDescriptor>();

            // The side effect of ordering the keys will be that they will also be in length order.
            // All parents of nested namespaces will be before their nested descendants.
            foreach (string name in namespaces.Keys.OrderBy(k => k))
            {
                bool first = true;
                foreach (Namespace ns in namespaces[name])
                {
                    if (first)
                    {
                        first = false;
                        builder.Add(name, new NamespaceDescriptor() { Name = name, UsingDefaultLocale = true, Representative = ns });
                    }

                    if (builder[name].UsingDefaultLocale && ns.NamesByLocale.ContainsKey(locale))
                    {
                        builder[name].UsingDefaultLocale = false;
                        builder[name].Representative = ns;
                        string localizedName = ns.NamesByLocale[locale];
                        string[] localizedParts = localizedName.Split('.');
                        string[] unlocalizedParts = name.Split('.');
                        if (localizedParts.Length != unlocalizedParts.Length)
                        {
                            throw new ParserException(ns.FirstToken, "@localized annotation value does not have the right number of segments.");
                        }

                        string nslookup = "";
                        for (int i = 0; i < localizedParts.Length; ++i)
                        {
                            nslookup += (i == 0 ? "" : ".") + unlocalizedParts[i];
                            if (builder[nslookup].UsingDefaultLocale)
                            {
                                builder[nslookup].Name = localizedParts[localizedParts.Length - 1];
                                builder[nslookup].Representative = ns;
                                builder[nslookup].UsingDefaultLocale = false;
                            }
                        }
                    }
                }
            }

            Dictionary<string, NamespaceReferenceTemplate> output = new Dictionary<string, NamespaceReferenceTemplate>();
            foreach (NamespaceDescriptor nd in builder.Values)
            {
                output[nd.Name] = new NamespaceReferenceTemplate() { Name = nd.Name, OriginalNamespace = nd.Representative };
            }

            return output;
        }

        private class NamespaceDescriptor
        {
            public string Name { get; set; }
            public bool UsingDefaultLocale { get; set; }
            public Namespace Representative { get; set; } // Real parsed namespace that will be used as a token proxy
        }
    }
}
