using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crayon;
using Crayon.ParseTree;

namespace Core.Platforms
{
	internal interface INativeTranslator
	{
		string TranslatePrint(ExpressionTranslator translator, Expression value);
	}
}
