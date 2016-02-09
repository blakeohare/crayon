using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibraryConfig
{
	public interface IPlatform
	{
		PlatformId PlatformId { get; }
		LanguageId LanguageId { get; }
		string Translate(object expressionObj);
		string DoReplacements(string code, Dictionary<string, string> replacements);
	}
}
