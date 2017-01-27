using System.Collections.Generic;
using System.Linq;

namespace Crayon.Pastel.Nodes
{
    class WhileLoop : Executable
    {
        public Expression Condition { get; set; }
        public Executable[] Code { get; set; }

        public WhileLoop(
            Token whileToken,
            Expression condition,
            IList<Executable> code) : base(whileToken)
        {
            this.Condition = condition;
            this.Code = code.ToArray();
        }
    }
}
