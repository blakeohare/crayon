using Crayon;
using Crayon.ParseTree;

namespace Core.Platforms
{
	class PythonPyGame : INativeTranslator
	{
		public string TranslatePrint(ExpressionTranslator translator, Expression value)
		{
			return "print(" + translator.Translate(value) + ")";
		}
	}
}
