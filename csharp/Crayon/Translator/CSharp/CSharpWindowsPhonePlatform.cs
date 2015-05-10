using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon.Translator.CSharp
{
	class CSharpWindowsPhonePlatform : CSharpPlatform
	{
		public CSharpWindowsPhonePlatform()
			: base(new CSharpWindowsPhoneSystemFunctionTranslator())
		{ }

		public override string OutputFolderName { get { return "cswinphone"; } }

		public override bool IsOpenGlBased { get { return false; } }
		public override bool SupportsGamePad { get { return false; } }

		public override void ApplyPlatformSpecificReplacements(Dictionary<string, string> replacements)
		{
		}

		public override void AddPlatformSpecificSystemLibraries(HashSet<string> systemLibraries)
		{
		}

		public override void PlatformSpecificFiles(string projectId, List<string> compileTargets, Dictionary<string, FileOutput> files, Dictionary<string, string> replacements)
		{
		}
	}
}
