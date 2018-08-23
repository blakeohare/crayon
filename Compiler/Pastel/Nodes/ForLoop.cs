using System.Collections.Generic;
using System.Linq;

namespace Pastel.Nodes
{
    internal class ForLoop : Executable
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

        public override Executable ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            this.InitCode = Executable.ResolveNamesAndCullUnusedCodeForBlock(this.InitCode, compiler).ToArray();
            this.Condition = this.Condition.ResolveNamesAndCullUnusedCode(compiler);
            this.StepCode = Executable.ResolveNamesAndCullUnusedCodeForBlock(this.StepCode, compiler).ToArray();

            // TODO: check Condition for falseness

            this.Code = Executable.ResolveNamesAndCullUnusedCodeForBlock(this.Code, compiler).ToArray();

            return this;
        }

        internal override void ResolveTypes(VariableScope varScope, PastelCompiler compiler)
        {
            // This gets compiled as a wihle loop with the init added before the loop, so it should go in the same variable scope.
            // The implication is that multiple declarations in the init for successive loops will collide.
            Executable.ResolveTypes(this.InitCode, varScope, compiler);
            this.Condition = this.Condition.ResolveType(varScope, compiler);
            Executable.ResolveTypes(this.StepCode, varScope, compiler);
            VariableScope innerScope = new VariableScope(varScope);
            Executable.ResolveTypes(this.Code, innerScope, compiler);
        }

        internal override Executable ResolveWithTypeContext(PastelCompiler compiler)
        {
            Executable.ResolveWithTypeContext(compiler, this.InitCode);
            this.Condition = this.Condition.ResolveWithTypeContext(compiler);
            Executable.ResolveWithTypeContext(compiler, this.StepCode);
            Executable.ResolveWithTypeContext(compiler, this.Code);

            // Canonialize the for loop into a while loop.
            List<Executable> loopCode = new List<Executable>(this.Code);
            loopCode.AddRange(this.StepCode);
            WhileLoop whileLoop = new WhileLoop(this.FirstToken, this.Condition, loopCode);
            loopCode = new List<Executable>(this.InitCode);
            loopCode.Add(whileLoop);
            return new ExecutableBatch(this.FirstToken, loopCode);
        }
    }
}
