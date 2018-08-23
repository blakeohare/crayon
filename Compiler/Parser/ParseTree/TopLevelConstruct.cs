using Localization;
using System;
using System.Collections.Generic;

namespace Parser.ParseTree
{
    public abstract class TopLevelConstruct : Node
    {
        public LibraryMetadata Library { get; set; }

        public TopLevelConstruct(Token firstToken, TopLevelConstruct owner, FileScope fileScope)
            : base(firstToken, owner)
        {
            this.fileScopeOverride = fileScope;
        }

        public abstract string GetFullyQualifiedLocalizedName(Locale locale);

        private static Dictionary<string, string[]> namespacePartCache = new Dictionary<string, string[]>();

        /*
            This is the namespace that this executable is housed in. It is an array of the full
            namespace name (with dots) as the first element. Successive elements are shortened versions
            of this by popping off each segment one by one.
            For example, if this is a function whose fully qualified name is Foo.Bar.Baz.myFunction, then
            the LocalNamespace will be [ "Foo.Bar.Baz", "Foo.Bar", "Foo" ].
        */
        private Dictionary<Locale, string[]> localNamespaceByLocale = new Dictionary<Locale, string[]>();
        public string[] GetWrappingNamespaceIncrements(Locale locale)
        {
            if (!this.localNamespaceByLocale.ContainsKey(locale))
            {
                TopLevelConstruct ownerWalker = this.Owner;
                while (ownerWalker != null && !(ownerWalker is Namespace))
                {
                    ownerWalker = ownerWalker.Owner;
                }

                string ns = ownerWalker == null ? "" : ownerWalker.GetFullyQualifiedLocalizedName(locale);
                if (!TopLevelConstruct.namespacePartCache.ContainsKey(ns))
                {
                    if (ns.Length == 0)
                    {
                        TopLevelConstruct.namespacePartCache[""] = new string[0];
                    }
                    else
                    {
                        string[] parts = ns.Split('.');
                        for (int i = 1; i < parts.Length; ++i)
                        {
                            parts[i] = parts[i - 1] + "." + parts[i];
                        }
                        Array.Reverse(parts);
                        TopLevelConstruct.namespacePartCache[ns] = parts;
                    }
                }
                this.localNamespaceByLocale[locale] = TopLevelConstruct.namespacePartCache[ns];
            }
            return this.localNamespaceByLocale[locale];
        }

        internal abstract void Resolve(ParserContext parser);
        internal abstract void ResolveEntityNames(ParserContext parser);
    }
}
