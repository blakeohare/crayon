using Crayon;
using Crayon.ParseTree;

namespace Core.Platforms
{
	class JavaAndroid : INativeTranslator
	{
		public string TranslatePrint(ExpressionTranslator translator, Expression value)
		{
			return "android.util.Log.d(\"\", " + translator.Translate(value) + ")";
		}
	}
}
