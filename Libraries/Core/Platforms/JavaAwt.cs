using Crayon;
using Crayon.ParseTree;

namespace Core.Platforms
{
	class JavaAwt : INativeTranslator
	{
		public string TranslatePrint(ExpressionTranslator translator, Expression value)
		{
			return "System.out.println(" + translator.Translate(value) + ")";
		}
	}
}
