using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibraryConfig
{
	public interface ILibraryConfig
	{
		string GetEmbeddedCode();
		string GetTranslationCode(IPlatform platform, string functionName);
		string TranslateNativeInvocation(IPlatform translator, string functionName, object[] args);
		Dictionary<string, string> GetSupplementalTranslatedCode();
	}
}
