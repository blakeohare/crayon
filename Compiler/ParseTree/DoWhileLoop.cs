using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
    internal class DoWhileLoop : Executable
    {
        public Executable[] Code { get; private set; }
        public Expression Condition { get; private set; }

        public DoWhileLoop(Token doToken, IList<Executable> code, Expression condition, Executable owner)
            : base(doToken, owner)
        {
            this.Code = code.ToArray();
            this.Condition = condition;
        }

        internal override IList<Executable> Resolve(Parser parser)
        {
            this.Code = Resolve(parser, this.Code).ToArray();

            this.Condition = this.Condition.Resolve(parser);

            return Listify(this);
        }

        internal override void GetAllVariableNames(Dictionary<string, bool> lookup)
        {
            foreach (Executable line in this.Code)
            {
                line.GetAllVariableNames(lookup);
            }
        }

        internal override void GenerateGlobalNameIdManifest(VariableIdAllocator varIds)
        {
            foreach (Executable ex in this.Code)
            {
                ex.GenerateGlobalNameIdManifest(varIds);
            }
        }

        internal override void CalculateLocalIdPass(VariableIdAllocator varIds)
        {
            foreach (Executable ex in this.Code)
            {
                ex.CalculateLocalIdPass(varIds);
            }
        }

        internal override void SetLocalIdPass(VariableIdAllocator varIds)
        {
            foreach (Executable ex in this.Code)
            {
                ex.SetLocalIdPass(varIds);
            }

            this.Condition.SetLocalIdPass(varIds);
        }

        internal override Executable ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
        {
            this.BatchExecutableNameResolver(parser, lookup, imports, this.Code);
            this.Condition = this.Condition.ResolveNames(parser, lookup, imports);
            return this;
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars)
        {
            this.Condition.GetAllVariablesReferenced(vars);
            for (int i = 0; i < this.Code.Length; ++i)
            {
                this.Code[i].GetAllVariablesReferenced(vars);
            }
        }
    }
}
