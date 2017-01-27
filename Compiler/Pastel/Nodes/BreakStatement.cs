using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon.Pastel.Nodes
{
    class BreakStatement : Executable
    {
        public BreakStatement(Token breakToken) : base(breakToken)
        { }
    }
}
