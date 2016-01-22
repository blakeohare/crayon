using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon
{
	public class ExpressionTranslator
	{
		private AbstractPlatform platform;

		internal ExpressionTranslator(AbstractPlatform platform)
		{
			this.platform = platform;
			this.Platform = platform.PlatformId;
			this.Language = platform.LanguageId;
		}

		public PlatformId Platform { get; private set; }
		public LanguageId Language { get; private set; }
		
		public string Translate(Crayon.ParseTree.Expression expression)
		{
			List<string> output = new List<string>();
			this.platform.Translator.TranslateExpression(output, expression);
			return string.Join("", output);
		}
	}
}
