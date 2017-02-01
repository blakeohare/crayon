using System;
using System.Collections.Generic;
using System.Linq;

namespace Pastel.Nodes
{
    class ForLoop : Executable
    {
        public Executable[] InitCode { get; set; }
        public Expression Condition { get; set; }
        public Executable[] StepCode { get; set; }
        public Executable[] Code { get; set; }

        public ForLoop(
            Token forToken,
            IList<Executable> initCode,
            Expression condition,
            IList<Executable> stepCode,
            IList<Executable> code) : base(forToken)
        {
            this.InitCode = initCode.ToArray();
            this.Condition = condition;
            this.StepCode = stepCode.ToArray();
            this.Code = code.ToArray();
        }

        public override IList<Executable> ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            this.InitCode = Executable.ResolveNamesAndCullUnusedCodeForBlock(this.InitCode, compiler).ToArray();
            this.Condition = this.Condition.ResolveNamesAndCullUnusedCode(compiler);
            this.StepCode = Executable.ResolveNamesAndCullUnusedCodeForBlock(this.StepCode, compiler).ToArray();

            // TODO: check Condition for falseness

            this.Code = Executable.ResolveNamesAndCullUnusedCodeForBlock(this.Code, compiler).ToArray();
            
            return Listify(this);
        }
    }
}