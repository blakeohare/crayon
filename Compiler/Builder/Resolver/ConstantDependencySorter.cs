using Builder.ParseTree;
using System.Collections.Generic;
using System.Linq;

namespace Builder.Resolver
{
    internal class ConstantDependencySorter
    {
        private CompilationScope currentScope;
        private ConstDefinition[] consts;
        private HashSet<ConstDefinition> resolved = new HashSet<ConstDefinition>();
        private HashSet<ConstDefinition> resolving = new HashSet<ConstDefinition>();
        private List<ConstDefinition> output = new List<ConstDefinition>();

        public ConstantDependencySorter(IEnumerable<ConstDefinition> constants)
        {
            this.consts = constants.ToArray();
            if (this.consts.Length > 0)
            {
                this.currentScope = this.consts[0].CompilationScope;
            }
        }

        public ConstDefinition[] SortConstants()
        {
            for (int i = 0; i < this.consts.Length; ++i)
            {
                ConstDefinition cnst = this.consts[i];
                if (!this.resolved.Contains(cnst))
                {
                    this.ResolveConstDependency(cnst);
                }
            }

            ConstDefinition[] output = this.output.ToArray();
            this.output.Clear();
            this.resolving.Clear();
            this.resolved.Clear();
            return output;
        }

        private void ResolveConstDependency(ConstDefinition cnst)
        {
            if (this.resolving.Contains(cnst))
            {
                throw new ParserException(cnst, "This constant has a dependency loop.");
            }

            this.resolving.Add(cnst);

            foreach (ConstReference cnstRef in cnst.Expression.GetFlattenedDescendants().OfType<ConstReference>())
            {
                if (cnstRef.CompilationScope == this.currentScope)
                {
                    this.ResolveConstDependency(cnstRef.ConstStatement);
                }
            }

            this.resolving.Remove(cnst);
            this.resolved.Add(cnst);
            this.output.Add(cnst);
        }
    }
}
