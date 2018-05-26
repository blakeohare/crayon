using Common;
using Pastel;
using Platform;
using System;
using System.Collections.Generic;

namespace JavaScriptAppIos
{
    public class PlatformImpl : AbstractPlatform
    {
        public override string InheritsFrom { get { return "javascript-app"; } }
        public override string Name { get { return "javascript-app-ios"; } }
        public override string NL { get { return "\n"; } }

        public PlatformImpl()
            : base(Language.JAVASCRIPT)
        { }

        public override void ExportProject(
            Dictionary<string, FileOutput> output,
            TemplateStorage templates,
            IList<LibraryForExport> libraries,
            ResourceDatabase resourceDatabase,
            Options options)
        {
            options.SetOption(ExportOptionKey.JS_FILE_PREFIX, null);
            options.SetOption(ExportOptionKey.JS_FULL_PAGE, false); // iOS export has its own enforced fullscreen logic
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
            Dictionary<string, FileOutput> basicProject = new Dictionary<string, FileOutput>();
            this.ParentPlatform.ExportProject(
                basicProject,
                templates,
                libraries,
                resourceDatabase,
                options);

            // TODO: not good. The library inclusions should automatically be populated in LangJavaScript platforms.
            // This is also done identically in the ChromeApp PlatformImpl.
            replacements["JS_LIB_INCLUSIONS"] = JavaScriptApp.PlatformImpl.GenerateJsLibInclusionHtml(basicProject.Keys);

            foreach (string filePath in basicProject.Keys)
            {
                files["%%%PROJECT_ID%%%/%%%PROJECT_ID%%%/jsres/" + filePath] = basicProject[filePath];
            }

            // TODO: use this in the pbxproj file.
            string uuidSeed = options.GetStringOrNull(ExportOptionKey.GUID_SEED);


            OrientationParser orientations = new OrientationParser(options);
            bool useLandscapeForLaunchscreen = orientations.SupportsLandscapeLeft || orientations.SupportsLandscapeRight;
            FileOutput launchScreen;
            if (options.GetBool(ExportOptionKey.HAS_LAUNCHSCREEN))
            {
                launchScreen = new FileOutput()
                {
                    Type = FileOutputType.Image,
                    Bitmap = new SystemBitmap(options.GetString(ExportOptionKey.LAUNCHSCREEN_PATH)),
                };
            }
            else
            {
                launchScreen = new FileOutput()
                {
                    Type = FileOutputType.Image,
                    Bitmap = new SystemBitmap(typeof(JavaScriptAppIos.PlatformImpl).Assembly, "SwiftResources/" +
                       (useLandscapeForLaunchscreen ? "launchhorizontal.png" : "launchvertical.png")),
                };
            }
            files["%%%PROJECT_ID%%%/%%%PROJECT_ID%%%/Assets.xcassets/Launchscreen.imageset/launchscreen.png"] = launchScreen;
            replacements["LAUNCH_SCREEN_WIDTH"] = launchScreen.Bitmap.Width.ToString();
            replacements["LAUNCH_SCREEN_HEIGHT"] = launchScreen.Bitmap.Height.ToString();

            IconSetGenerator icons = new IconSetGenerator();
            if (options.GetBool(ExportOptionKey.HAS_ICON))
            {
                string iconPath = options.GetString(ExportOptionKey.ICON_PATH);
                SystemBitmap icon = new SystemBitmap(iconPath);
                icons.AddInputImage(icon);
            }

            Dictionary<int, SystemBitmap> iconImagesBySize = icons
                .AddOutputSize(20 * 1)
                .AddOutputSize(20 * 2)
                .AddOutputSize(20 * 3)
                .AddOutputSize(29 * 1)
                .AddOutputSize(29 * 2)
                .AddOutputSize(29 * 3)
                .AddOutputSize(40 * 1)
                .AddOutputSize(40 * 2)
                .AddOutputSize(40 * 3)
                .AddOutputSize(60 * 2)
                .AddOutputSize(60 * 3)
                .AddOutputSize(76 * 1)
                .AddOutputSize(76 * 2)
                .AddOutputSize(167) // 83.5 * 2
                .AddOutputSize(1024)
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
                "%%%PROJECT_ID%%%/%%%PROJECT_ID%%%/Assets.xcassets/Launchscreen.imageset/Contents.json|SwiftResources/ImageSetContentJson.txt",
                "%%%PROJECT_ID%%%/%%%PROJECT_ID%%%/Base.lproj/LaunchScreen.storyboard|SwiftResources/LaunchScreenStoryboard.txt",
                "%%%PROJECT_ID%%%/%%%PROJECT_ID%%%/Base.lproj/Main.storyboard|SwiftResources/MainStoryboard.txt",
                "%%%PROJECT_ID%%%/%%%PROJECT_ID%%%/Info.plist|SwiftResources/InfoPlist.txt",
                "%%%PROJECT_ID%%%/%%%PROJECT_ID%%%/ViewController.swift|SwiftResources/ViewControllerSwift.txt",
                "%%%PROJECT_ID%%%/%%%PROJECT_ID%%%/jsres/ios.js|SwiftResources/iOSjs.txt",
                "%%%PROJECT_ID%%%/%%%PROJECT_ID%%%/jsres/index.html|SwiftResources/HostHtml.txt",
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

            foreach (string filename in files.Keys)
            {
                output[this.ApplyReplacements(filename, replacements)] = files[filename];
            }
        }

        public override void ExportStandaloneVm(
            Dictionary<string, FileOutput> output,
            TemplateStorage templates,
            IList<LibraryForExport> everyLibrary)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<string, string> GenerateReplacementDictionary(
            Options options,
            ResourceDatabase resDb)
        {
            Dictionary<string, string> replacements = this.ParentPlatform.GenerateReplacementDictionary(options, resDb);
            replacements["ORGANIZATION_NAME"] = "Organization Name";

            replacements["DEVELOPMENT_TEAM_ALL_CAPS"] = "";
            replacements["DEVELOPMENT_TEAM_NOT_CAPS"] = "";
            string developmentTeam = options.GetStringOrNull(ExportOptionKey.IOS_DEV_TEAM_ID);
            if (developmentTeam != null)
            {
                replacements["DEVELOPMENT_TEAM_NOT_CAPS"] = "DevelopmentTeam = " + developmentTeam + ";";
                replacements["DEVELOPMENT_TEAM_ALL_CAPS"] = "DEVELOPMENT_TEAM = " + developmentTeam + ";";
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
