using Crayon;
using Crayon.ParseTree;

namespace Core.Platforms
{
	class JavaScriptCanvas : INativeTranslator
	{
		public string TranslatePrint(ExpressionTranslator translator, Expression value)
		{
			return "R.print(" + translator.Translate(value) + ")";
		}
	}
}
