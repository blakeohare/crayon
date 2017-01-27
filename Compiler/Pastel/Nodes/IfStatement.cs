using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon.Pastel.Nodes
{
    class IfStatement : Executable
    {
        public IfStatement(Token ifToken, Expression condition, IList<Executable> ifCode, Token elseToken, IList<Executable> elseCode)
        {

        }
    }
}
