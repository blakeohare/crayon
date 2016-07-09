using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
    internal class WhileLoop : Executable
    {
        public Expression Condition { get; private set; }
        public Executable[] Code { get; private set; }

        public WhileLoop(Token whileToken, Expression condition, IList<Executable> code, Executable owner)
            : base(whileToken, owner)
        {
            this.Condition = condition;
            this.Code = code.ToArray();
        }

        internal override IList<Executable> Resolve(Parser parser)
        {
            this.Condition = this.Condition.Resolve(parser);
            this.Code = Resolve(parser, this.Code).ToArray();
            return Listify(this);
        }

        internal override Executable ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
        {
            this.Condition = this.Condition.ResolveNames(parser, lookup, imports);
            this.BatchExecutableNameResolver(parser, lookup, imports, this.Code);
            return this;
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
            for (int i = 0; i < this.Code.Length; ++i)
            {
                this.Code[i].CalculateLocalIdPass(varIds);
            }
        }

        internal override void SetLocalIdPass(VariableIdAllocator varIds)
        {
            this.Condition.SetLocalIdPass(varIds);
            for (int i = 0; i < this.Code.Length; ++i)
            {
                this.Code[i].SetLocalIdPass(varIds);
            }
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars)
        {
            this.Condition.GetAllVariablesReferenced(vars);
            foreach (Executable ex in this.Code)
            {
                ex.GetAllVariablesReferenced(vars);
            }
        }
    }
}
