using System;
using System.Collections.Generic;

namespace Crayon.ParseTree
{
    // Despite being an "Executable", this isn't an executable thing.
    // It will get optimized away at resolution time.
    public class Namespace : TopLevelConstruct
    {
        public TopLevelConstruct[] Code { get; set; }
        public string Name { get; set; }

        public Namespace(
            Token namespaceToken,
            string name,
            TopLevelConstruct owner,
            LibraryMetadata library,
            FileScope fileScope)
            : base(namespaceToken, owner, fileScope)
        {
            this.Library = library;
            this.Name = name;
        }

        public void GetFlattenedCode(IList<TopLevelConstruct> executableOut)
        {
            foreach (TopLevelConstruct item in this.Code)
            {
                if (item is Namespace)
                {
                    ((Namespace)item).GetFlattenedCode(executableOut);
                }
                else
                {
                    // already filtered at parse time to correct member types.
                    executableOut.Add(item);
                }
            }
        }

        internal override void Resolve(ParserContext parser)
        {
            throw new ParserException(this.FirstToken, "Namespace declaration not allowed here. Namespaces may only exist in the root of a file or nested within other namespaces.");
        }

        internal override void ResolveNames(ParserContext parser)
        {
            throw new InvalidOperationException();
        }

        internal override void GetAllVariableNames(Dictionary<string, bool> lookup)
        {
            throw new NotImplementedException();
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }
        internal override void PerformLocalIdAllocation(ParserContext parser, VariableIdAllocator varIds, VariableIdAllocPhase phase)
        {
            // Not called in this way.
            throw new NotImplementedException();
        }
    }
}
