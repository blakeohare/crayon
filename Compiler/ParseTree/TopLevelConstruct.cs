using System;
using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
    public abstract class TopLevelConstruct : Node
    {
        public Library Library { get; set; }
        public string[] NamespacePrefixSearch { get; set; }

        public TopLevelConstruct(Token firstToken, TopLevelConstruct owner) : base(firstToken, owner)
        {

        }

        private static Dictionary<string, string[]> namespacePartCache = new Dictionary<string, string[]>();

        /*
            This is the namespace that this executable is housed in (only applicable to top-level
            executables such as functions, classes, constants, enums). It is an array of the full
            namespace name (with dots) as the first element. Successive elements are shortened versions
            of this by popping off each segment one by one.
            For example, if this is a function whose fully qualified name is Foo.Bar.Baz.myFunction, then
            the LocalNamespace will be [ "Foo.Bar.Baz", "Foo.Bar", "Foo" ].
        */
        private string[] localNamespace = null;
        public string[] LocalNamespace
        {
            get
            {
                if (this.localNamespace == null)
                {
                    if (!TopLevelConstruct.namespacePartCache.ContainsKey(this.Namespace ?? ""))
                    {
                        if (this.Namespace == null || this.Namespace.Length == 0)
                        {
                            TopLevelConstruct.namespacePartCache[""] = new string[0];
                        }
                        else
                        {
                            string[] parts = this.Namespace.Split('.');
                            for (int i = 1; i < parts.Length; ++i)
                            {
                                parts[i] = parts[i - 1] + "." + parts[i];
                            }
                            Array.Reverse(parts);
                            TopLevelConstruct.namespacePartCache[this.Namespace] = parts;
                        }
                    }
                    this.localNamespace = TopLevelConstruct.namespacePartCache[this.Namespace ?? ""];
                }
                return this.localNamespace;
            }
        }
        public string Namespace { get; set; }

        internal abstract void Resolve(Parser parser);
        internal abstract void ResolveNames(Parser parser, Dictionary<string, TopLevelConstruct> lookup, string[] imports);
        internal abstract void GetAllVariablesReferenced(HashSet<Variable> vars);
        internal override void GetAllVariableNames(Dictionary<string, bool> lookup)
        {
            throw new Exception(); // Not used
        }

        internal override void PerformLocalIdAllocation(Parser parser, VariableIdAllocator varIds, VariableIdAllocPhase phase)
        {
            throw new NotImplementedException();
        }
    }
}
