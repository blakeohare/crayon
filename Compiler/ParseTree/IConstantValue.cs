using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon.ParseTree
{
	internal interface IConstantValue
	{
		Expression CloneValue(Token token, Executable owner);
	}
}
