using System.Collections.Generic;
using System.Linq;

namespace Crayon.Pastel.Nodes
{
    class IfStatement : Executable
    {
        public Expression Condition { get; set; }
        public Executable[] IfCode { get; set; }
        public Token ElseToken { get; set; }
        public Executable[] ElseCode { get; set; }

        public IfStatement(
            Token ifToken,
            Expression condition,
            IList<Executable> ifCode,
            Token elseToken,
            IList<Executable> elseCode) : base(ifToken)
        {
            this.Condition = condition;
            this.IfCode = ifCode.ToArray();
            this.ElseToken = elseToken;
            this.ElseCode = elseCode.ToArray();
        }
    }
}
