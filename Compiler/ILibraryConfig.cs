using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crayon.ParseTree;

namespace Crayon
{
	public interface ILibraryConfig
	{
		string GetEmbeddedCode();
		string GetTranslationCode(string functionName);
		string TranslateNativeInvocation(ExpressionTranslator translator, string functionName, Expression[] args);
	}
}
