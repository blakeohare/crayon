using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Pastel.Nodes;
using Platform;

namespace JavaScriptAppIos
{
    public class PlatformImpl : Platform.AbstractPlatform
    {
        public override string InheritsFrom { get { return "javascript-app-gl"; } }
        public override string Name { get { return "javascript-app-ios"; } }
        public override string NL { get { return "\n"; } }

		public override Dictionary<string, FileOutput> ExportProject(
			IList<VariableDeclaration> globals,
			IList<StructDefinition> structDefinitions,
			IList<FunctionDefinition> functionDefinitions,
			IList<LibraryForExport> libraries,
			ResourceDatabase resourceDatabase,
			Options options,
			ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform)
		{
			options.SetOption(ExportOptionKey.JS_FILE_PREFIX, null);
			Dictionary<string, string> replacements = this.GenerateReplacementDictionary(options, resourceDatabase);
			Dictionary<string, FileOutput> files = new Dictionary<string, FileOutput>();
			Dictionary<string, FileOutput> basicProject = this.ParentPlatform.ExportProject(
				globals,
				structDefinitions,
				functionDefinitions,
				libraries,
				resourceDatabase,
				options,
				libraryNativeInvocationTranslatorProviderForPlatform);
			foreach (string filePath in basicProject.Keys)
			{
				files["CrayonApp/CrayonApp/jsres/" + filePath] = basicProject[filePath];
			}

			// TODO: use this in the pbxproj file.
			string uuidSeed = options.GetStringOrNull(ExportOptionKey.GUID_SEED);

			foreach (string pair in new string[] {
				"CrayonApp/CrayonApp.xcodeproj/project.pbxproj|SwiftResources/PbxProj.txt",
				"CrayonApp/CrayonApp/AppDelegate.swift|SwiftResources/AppDelegateSwift.txt",
				"CrayonApp/CrayonApp/Assets.xcassets/AppIcon.appiconset/Contents.json|SwiftResources/IconSetContentJson.txt",
				"CrayonApp/CrayonApp/Base.lproj/LaunchScreen.storyboard|SwiftResources/LaunchScreenStoryboard.txt",
				"CrayonApp/CrayonApp/Base.lproj/Main.storyboard|SwiftResources/MainStoryboard.txt",
				"CrayonApp/CrayonApp/Info.plist|SwiftResources/InfoPlist.txt",
				"CrayonApp/CrayonApp/ViewController.swift|SwiftResources/ViewControllerSwift.txt",
			})
			{
				string[] parts = pair.Split('|');
				files[parts[0]] = new FileOutput()
				{
					TrimBomIfPresent = true,
					Type = FileOutputType.Text,
					TextContent = this.LoadTextResource(parts[1], replacements),
				};
			}

			return files;
		}

        public override Dictionary<string, FileOutput> ExportStandaloneVm(
            IList<VariableDeclaration> globals,
            IList<StructDefinition> structDefinitions,
            IList<FunctionDefinition> functionDefinitions,
            IList<LibraryForExport> everyLibrary,
            ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform)
        {
            throw new NotImplementedException();
        }

        public override string GenerateCodeForFunction(AbstractTranslator translator, FunctionDefinition funcDef)
        {
            throw new NotImplementedException();
        }

        public override string GenerateCodeForGlobalsDefinitions(AbstractTranslator translator, IList<VariableDeclaration> globals)
        {
            throw new NotImplementedException();
        }

        public override string GenerateCodeForStruct(StructDefinition structDef)
        {
			throw new NotImplementedException();
        }

        public override Dictionary<string, string> GenerateReplacementDictionary(Options options, ResourceDatabase resDb)
        {
			return this.ParentPlatform.GenerateReplacementDictionary(options, resDb);
        }

        public override IDictionary<string, object> GetConstantFlags()
        {
			return this.ParentPlatform.GetConstantFlags();
        }
    }
}
