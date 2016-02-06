using Crayon;
using Crayon.ParseTree;

namespace Core.Platforms
{
	class CSharpOpenTk : INativeTranslator
	{
		public string TranslatePrint(ExpressionTranslator translator, Expression value)
		{
			return "System.Console.WriteLine(" + translator.Translate(value) + ")";
		}
	}
}
