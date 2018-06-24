using Common;
using Pastel;
using Platform;
using System;
using System.Collections.Generic;

namespace CSharpAppUwp
{
	public class PlatformImpl : AbstractPlatform
	{
		public override string Name { get { return "csharp-app-uwp"; } }
		public override string InheritsFrom { get { return "lang-csharp"; } }
		public override string NL { get { return "\r\n"; } }

		public PlatformImpl()
			: base(Language.CSHARP)
		{ }

		public override IDictionary<string, object> GetConstantFlags()
		{
			return new Dictionary<string, object>();
		}

		public override Dictionary<string, string> GenerateReplacementDictionary(Options options, ResourceDatabase resDb)
		{
			return this.ParentPlatform.GenerateReplacementDictionary(options, resDb);
		}

		public override void ExportStandaloneVm(
			Dictionary<string, FileOutput> output,
			TemplateStorage templates,
			IList<LibraryForExport> everyLibrary)
		{
			throw new NotImplementedException();
		}

		public override void ExportProject(
			Dictionary<string, FileOutput> output,
			TemplateStorage templates,
			IList<LibraryForExport> libraries,
			ResourceDatabase resourceDatabase,
			Options options)
		{
			Dictionary<string, string> replacements = this.GenerateReplacementDictionary(options, resourceDatabase);

			string projectId = replacements["PROJECT_ID"];

			this.CopyResourceAsText(output, projectId + ".sln", "Resources/SolutionFile.txt", replacements);
			this.CopyResourceAsText(output, projectId + "/" + projectId + ".csproj", "Resources/ProjectFile.txt", replacements);

			this.CopyResourceAsText(output, projectId + "/App.xaml", "Resources/AppXaml.txt", replacements);
			this.CopyResourceAsText(output, projectId + "/App.xaml.cs", "Resources/AppXamlCs.txt", replacements);
			this.CopyResourceAsText(output, projectId + "/MainPage.xaml", "Resources/MainPageXaml.txt", replacements);
			this.CopyResourceAsText(output, projectId + "/MainPage.xaml.cs", "Resources/MainPageXamlCs.txt", replacements);
			this.CopyResourceAsText(output, projectId + "/Package.appxmanifest", "Resources/PackageAppxmanifest.txt", replacements);

			this.CopyResourceAsText(output, projectId + "/Properties/AssemblyInfo.cs", "Resources/AssemblyInfoCs.txt", replacements);
			this.CopyResourceAsText(output, projectId + "/Properties/Default.rd.xml", "Resources/DefaultRdXml.txt", replacements);

			this.CopyResourceAsBinary(output, projectId + "/Assets/LockScreenLogo.scale-200.png", "Resources/Assets/lockscreenlogoscale200.png");
			this.CopyResourceAsBinary(output, projectId + "/Assets/SplashScreen.scale-200.png", "Resources/Assets/splashscreenscale200.png");
			this.CopyResourceAsBinary(output, projectId + "/Assets/Square150x150Logo.scale-200.png", "Resources/Assets/square150x150logoscale200.png");
			this.CopyResourceAsBinary(output, projectId + "/Assets/Square44x44Logo.scale-200.png", "Resources/Assets/square44x44logo.png");
			this.CopyResourceAsBinary(output, projectId + "/Assets/Square44x44Logo.targetsize-24_altform-unplated.png", "Resources/Assets/square44x44logotargetsize24altformunplated.png");
			this.CopyResourceAsBinary(output, projectId + "/Assets/StoreLogo.png", "Resources/Assets/storelogo.png");
			this.CopyResourceAsBinary(output, projectId + "/Assets/Wide310x150Logo.scale-200.png", "Resources/Assets/wide310x150logoscale200.png");
		}
	}
}
