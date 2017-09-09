using System;
using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
    public abstract class Executable : Node
    {
        public static readonly Executable[] EMPTY_ARRAY = new Executable[0];

        public string[] NamespacePrefixSearch { get; set; }
        public Library Library { get; set; }

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
                    if (!Executable.namespacePartCache.ContainsKey(this.Namespace ?? ""))
                    {
                        if (this.Namespace == null || this.Namespace.Length == 0)
                        {
                            Executable.namespacePartCache[""] = new string[0];
                        }
                        else
                        {
                            string[] parts = this.Namespace.Split('.');
                            for (int i = 1; i < parts.Length; ++i)
                            {
                                parts[i] = parts[i - 1] + "." + parts[i];
                            }
                            Array.Reverse(parts);
                            Executable.namespacePartCache[this.Namespace] = parts;
                        }
                    }
                    this.localNamespace = Executable.namespacePartCache[this.Namespace ?? ""];
                }
                return this.localNamespace;
            }
        }
        public string Namespace { get; set; }

        public Executable(Token firstToken, TopLevelConstruct owner)
            : base(firstToken, owner)
        {
            if (owner != null)
            {
                this.Library = owner.Library;
            }
        }

        public virtual bool IsTerminator { get { return false; } }

        internal abstract IList<Executable> Resolve(Parser parser);
        internal abstract Executable ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports);
        internal abstract void GetAllVariablesReferenced(HashSet<Variable> vars);
        internal abstract Executable PastelResolve(Parser parser);

        internal static IList<Executable> Resolve(Parser parser, IList<Executable> executables)
        {
            List<Executable> output = new List<Executable>();
            foreach (Executable executable in executables)
            {
                output.AddRange(executable.Resolve(parser));
            }
            return output;
        }

        // The reason why I still run this function with actuallyDoThis = false is so that other platforms can still be exported to
        // and potentially crash if the implementation was somehow broken on Python (or some other future platform that doesn't have traditional switch statements).
        internal static Executable[] RemoveBreaksForElifedSwitch(bool actuallyDoThis, IList<Executable> executables)
        {
            List<Executable> newCode = new List<Executable>(executables);
            if (newCode.Count == 0) throw new Exception("A switch statement contained a case that had no code and a fallthrough.");
            if (newCode[newCode.Count - 1] is BreakStatement)
            {
                newCode.RemoveAt(newCode.Count - 1);
            }
            else if (newCode[newCode.Count - 1] is ReturnStatement)
            {
                // that's okay.
            }
            else
            {
                throw new Exception("A switch statement contained a case with a fall through.");
            }

            foreach (Executable executable in newCode)
            {
                if (executable is BreakStatement)
                {
                    throw new Exception("Can't break out of case other than at the end.");
                }
            }

            return actuallyDoThis ? newCode.ToArray() : executables.ToArray();
        }

        protected static IList<Executable> Listify(Executable ex)
        {
            return new Executable[] { ex };
        }

        // To be overridden if necessary.
        internal override void GetAllVariableNames(Dictionary<string, bool> lookup)
        { }

        internal virtual IList<Executable> PastelResolveComposite(Parser parser)
        {
            return Listify(this.PastelResolve(parser));
        }

        internal static Executable[] PastelResolveExecutables(Parser parser, IList<Executable> code)
        {
            if (code == null) return null;
            List<Executable> output = new List<Executable>();
            foreach (Executable line in code)
            {
                output.AddRange(line.PastelResolveComposite(parser));
            }
            return output.ToArray();
        }

    }
}
