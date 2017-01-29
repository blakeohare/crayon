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

        public override IList<Executable> NameResolution(Dictionary<string, FunctionDefinition> functionLookup, Dictionary<string, StructDefinition> structLookup)
        {
            throw new NotImplementedException();
        }

        public override void ResolveTypes()
        {
            throw new NotImplementedException();
        }
    }
}
