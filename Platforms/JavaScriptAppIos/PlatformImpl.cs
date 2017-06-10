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
            options.SetOption(ExportOptionKey.JS_HEAD_EXTRAS, string.Join(
                "\n",
                "<script type=\"text/javascript\" src=\"ios.js\"></script>",
                "<style type=\"text/css\">",
                "  body { margin:0px; background-color:#000; }",
                "  #crayon_host {",
                "    background-color:#000;",
				"    text-align:left;",
                "    width:100%;",
                "    height:100%;",
                "  }",
                "</style>"
            ));
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
                files["%%%PROJECT_ID%%%/%%%PROJECT_ID%%%/jsres/" + filePath] = basicProject[filePath];
            }

            // TODO: use this in the pbxproj file.
            string uuidSeed = options.GetStringOrNull(ExportOptionKey.GUID_SEED);

            foreach (string pair in new string[] {
                "%%%PROJECT_ID%%%/%%%PROJECT_ID%%%.xcodeproj/project.pbxproj|SwiftResources/PbxProj.txt",
                "%%%PROJECT_ID%%%/%%%PROJECT_ID%%%/AppDelegate.swift|SwiftResources/AppDelegateSwift.txt",
                "%%%PROJECT_ID%%%/%%%PROJECT_ID%%%/Assets.xcassets/AppIcon.appiconset/Contents.json|SwiftResources/IconSetContentJson.txt",
                "%%%PROJECT_ID%%%/%%%PROJECT_ID%%%/Base.lproj/LaunchScreen.storyboard|SwiftResources/LaunchScreenStoryboard.txt",
                "%%%PROJECT_ID%%%/%%%PROJECT_ID%%%/Base.lproj/Main.storyboard|SwiftResources/MainStoryboard.txt",
                "%%%PROJECT_ID%%%/%%%PROJECT_ID%%%/Info.plist|SwiftResources/InfoPlist.txt",
                "%%%PROJECT_ID%%%/%%%PROJECT_ID%%%/ViewController.swift|SwiftResources/ViewControllerSwift.txt",
                "%%%PROJECT_ID%%%/%%%PROJECT_ID%%%/jsres/ios.js|SwiftResources/iOSjs.txt",
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

            Dictionary<string, FileOutput> filesFixed = new Dictionary<string, FileOutput>();
            foreach (string filename in files.Keys)
            {
                filesFixed[this.ApplyReplacements(filename, replacements)] = files[filename];
            }

            return filesFixed;
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
            Dictionary<string, string> replacements = this.ParentPlatform.GenerateReplacementDictionary(options, resDb);
            replacements["ORGANIZTION_NAME"] = "Organization Name";
            return replacements;
        }

        public override IDictionary<string, object> GetConstantFlags()
        {
            return this.ParentPlatform.GetConstantFlags();
        }
    }
}
