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
        public override string InheritsFrom { get { return "javascript-app"; } }
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

            IconSetGenerator icons = new IconSetGenerator();
            if (options.GetBool(ExportOptionKey.HAS_ICON))
            {
                string iconPath = options.GetString(ExportOptionKey.ICON_PATH);
                SystemBitmap icon = new SystemBitmap(iconPath);
                icons.AddInputImage(icon);
            }

            Dictionary<int, SystemBitmap> iconImagesBySize = icons
                .AddOutputSize(29 * 1)
                .AddOutputSize(29 * 2)
                .AddOutputSize(29 * 3)
                .AddOutputSize(40 * 2)
                .AddOutputSize(40 * 3)
                .AddOutputSize(60 * 2)
                .AddOutputSize(60 * 3)
                .AddOutputSize(76 * 1)
                .AddOutputSize(76 * 2)
                .GenerateWithDefaultFallback();
            foreach (int size in iconImagesBySize.Keys)
            {
                files["%%%PROJECT_ID%%%/%%%PROJECT_ID%%%/Assets.xcassets/AppIcon.appiconset/icon" + size + ".png"] = new FileOutput()
                {
                    Type = FileOutputType.Image,
                    Bitmap = iconImagesBySize[size],
                };
            }

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
            replacements["ORGANIZATION_NAME"] = "Organization Name";

            replacements["DEVELOPMENT_TEAM_ALL_CAPS"] = "";
            replacements["DEVELOPMENT_TEAM_NOT_CAPS"] = "";
            string developmentTeam = options.GetStringOrNull(ExportOptionKey.IOS_DEV_TEAM_ID);
            if (developmentTeam != null)
            {
                replacements["DEVELOPMENT_TEAM_ALL_CAPS"] = "DevelopmentTeam = " + developmentTeam + ";";
                replacements["DEVELOPMENT_TEAM_NOT_CAPS"] = "DEVELOPMENT_TEAM = " + developmentTeam + ";";
            }

            string bundleIdPrefix = options.GetStringOrNull(ExportOptionKey.IOS_BUNDLE_PREFIX);
            replacements["IOS_BUNDLE_ID"] = bundleIdPrefix == null
                ? options.GetString(ExportOptionKey.PROJECT_ID)
                : bundleIdPrefix + "." + options.GetString(ExportOptionKey.PROJECT_ID);

            OrientationParser orientations = new OrientationParser(options);
            replacements["IOS_SUPPORTED_ORIENTATIONS_INFO_PLIST"] = string.Join("",
                "\t<array>\n",
                orientations.SupportsPortrait ? "\t\t<string>UIInterfaceOrientationPortrait</string>\n" : "",
                orientations.SupportsUpsideDown ? "\t\t<string>UIInterfaceOrientationPortraitUpsideDown</string>\n" : "",
                orientations.SupportsLandscapeLeft ? "\t\t<string>UIInterfaceOrientationLandscapeLeft</string>\n" : "",
                orientations.SupportsLandscapeRight ? "\t\t<string>UIInterfaceOrientationLandscapeRight</string>\n" : "",
                "\t</array>");

            return replacements;
        }

        public override IDictionary<string, object> GetConstantFlags()
        {
            return this.ParentPlatform.GetConstantFlags();
        }

        public override void GleanInformationFromPreviouslyExportedProject(Options options, string outputDirectory)
        {
            string pbxproj = FileUtil.JoinPath(
                outputDirectory,
                options.GetStringOrEmpty(ExportOptionKey.PROJECT_ID),
                options.GetStringOrEmpty(ExportOptionKey.PROJECT_ID) + ".xcodeproj",
                "project.pbxproj");
            if (FileUtil.FileExists(pbxproj))
            {
                string contents = FileUtil.ReadFileText(pbxproj);
                int devTeam = contents.IndexOf("DevelopmentTeam = ");
                if (devTeam != -1)
                {
                    devTeam += "DevelopmentTeam = ".Length;
                    int devTeamEnd = contents.IndexOf(';', devTeam);
                    if (devTeamEnd != -1)
                    {
                        string value = contents.Substring(devTeam, devTeamEnd - devTeam).Trim();
                        if (value.Length > 0 && !value.Contains("\n"))
                        {
                            options.SetOption(ExportOptionKey.IOS_DEV_TEAM_ID, value);
                        }
                    }
                }
            }
        }
    }
}
