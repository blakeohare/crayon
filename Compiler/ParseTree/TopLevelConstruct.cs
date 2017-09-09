using System;
using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
    public class TopLevelConstruct : Executable
    {
        public TopLevelConstruct(Token firstToken, Executable owner) : base(firstToken, owner)
        {

        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars)
        {
            throw new NotImplementedException();
        }

        internal override Executable PastelResolve(Parser parser)
        {
            throw new NotImplementedException();
        }

        internal override void PerformLocalIdAllocation(Parser parser, VariableIdAllocator varIds, VariableIdAllocPhase phase)
        {
            throw new NotImplementedException();
        }

        internal IList<TopLevelConstruct> ResolveTopLevel(Parser parser)
        {
            return new List<TopLevelConstruct>(this.Resolve(parser).Cast<TopLevelConstruct>());
        }

        internal override IList<Executable> Resolve(Parser parser)
        {
            throw new NotImplementedException();
        }

        internal override Executable ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
        {
            throw new NotImplementedException();
        }
    }
}
