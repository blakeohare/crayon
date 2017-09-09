using System;
using System.Collections.Generic;

namespace Crayon.ParseTree
{
    // Despite being an "Executable", this isn't an executable thing.
    // It will get optimized away at resolution time.
    internal class Namespace : TopLevelConstruct
    {
        public TopLevelConstruct[] Code { get; set; }
        public string Name { get; set; }

        public Namespace(Token namespaceToken, string name, TopLevelConstruct owner, Library library)
            : base(namespaceToken, owner)
        {
            this.Library = library;
            this.Name = name;
        }

        public void GetFlattenedCode(IList<TopLevelConstruct> executableOut, string[] imports)
        {
            List<string> importsBuilder = new List<string>() { this.Name };
            importsBuilder.AddRange(imports);
            imports = importsBuilder.ToArray();

            foreach (TopLevelConstruct item in this.Code)
            {
                item.NamespacePrefixSearch = imports;

                if (item is Namespace)
                {
                    ((Namespace)item).GetFlattenedCode(executableOut, imports);
                }
                else
                {
                    // already filtered at parse time to correct member types.
                    executableOut.Add(item);
                }
            }
        }

        internal override IList<Executable> Resolve(Parser parser)
        {
            throw new ParserException(this.FirstToken, "Namespace declaration not allowed here. Namespaces may only exist in the root of a file or nested within other namespaces.");
        }

        internal override Executable ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
        {
            throw new InvalidOperationException();
        }

        internal override void GetAllVariableNames(Dictionary<string, bool> lookup)
        {
            throw new NotImplementedException();
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }
        internal override void PerformLocalIdAllocation(Parser parser, VariableIdAllocator varIds, VariableIdAllocPhase phase)
        {
            // Not called in this way.
            throw new NotImplementedException();
        }

        internal override Executable PastelResolve(Parser parser)
        {
            throw new System.NotImplementedException();
        }
    }
}
