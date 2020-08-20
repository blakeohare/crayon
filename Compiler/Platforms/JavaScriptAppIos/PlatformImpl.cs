using Common;
using CommonUtil.Disk;
using CommonUtil.Images;
using CommonUtil.Resources;
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
            : base("JAVASCRIPT")
        { }

        public override void ExportProject(
            Dictionary<string, FileOutput> output,
            string byteCode,
            IList<LibraryForExport> libraries,
            Build.ResourceDatabase resourceDatabase,
            Options options)
        {
            options.SetOption(ExportOptionKey.JS_FILE_PREFIX, null);
            options.SetOption(ExportOptionKey.JS_FULL_PAGE, false); // iOS export has its own enforced fullscreen logic
            options.SetOption(ExportOptionKey.JS_HEAD_EXTRAS, "");
            Dictionary<string, string> replacements = this.GenerateReplacementDictionary(options, resourceDatabase);

            string appName = options.GetString(ExportOptionKey.PROJECT_ID).ToLowerInvariant();
            foreach (string t in new string[] {
                appName + "/Actions/AbstractAction.swift|app_actions_abstractaction.swift",
                appName + "/Actions/ConsoleLogAction.swift|app_actions_consolelogaction.swift",
                appName + "/Actions/HttpAction.swift|app_actions_httpaction.swift",
                appName + "/AppDelegate.swift|app_appdelegate.swift",
                appName + "/Assets.xcassets/AppIcon.appiconset/Contents.json|app_assets_xcassets_appicon_appiconset_contents_json.txt",
                appName + "/Assets.xcassets/Contents.json|app_assets_xcassets_contents_json.txt",
                appName + "/Base.lproj/LaunchScreen.storyboard|app_base_lproj_launchscreen_storyboard.txt",
                appName + "/ContentView.swift|app_contentview.swift",
                appName + "/Info.plist|app_info_plist.txt",
                appName + "/jsres/crayon/ios.js|app_jsres_ios.js",
                appName + "/MessageSender.swift|app_messagesender.swift",
                appName + "/Preview Content/Preview Assets.xcassets/Contents.json|app_preview_content_contents_json.txt",
                appName + "/SceneDelegate.swift|app_scenedelegate.swift",
                appName + ".xcodeproj/project.pbxproj|xcodeproj_project_pbxproj.txt",
                appName + ".xcodeproj/project.xcworkspace/.gitignore|xcodeproj_project_xcworkspace_gitignore.txt",
                appName + ".xcodeproj/project.xcworkspace/xcshareddata/IDEWorkspaceChecks.plist|xcodeproj_project_xcworkspace_xcshareddata_ideworkspacechecks_plist.txt",
                appName + ".xcodeproj/xcshareddata/xcschemes/" + appName + ".xcscheme|xcodeproj_xcshareddata_xcschemes_app_xcscheme.txt",
            })
            {
                string[] parts = t.Split('|');
                output[parts[0]] = new FileOutput()
                {
                    TrimBomIfPresent = true,
                    Type = FileOutputType.Text,
                    TextContent = this.LoadTextResource("ResourcesIos/" + parts[1], replacements),
                };
            }

            output[appName + "/jsres/crayon/index.html"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = "<html><body>Hello, World?</body></html>",
            };

            this.GenerateIcons(appName, options, output);
        }

        private void GenerateIcons(string appName, Options options, Dictionary<string, FileOutput> files)
        {

            IconSetGenerator icons = new IconSetGenerator();
            if (options.GetBool(ExportOptionKey.HAS_ICON))
            {
                string[] iconPaths = options.GetStringArray(ExportOptionKey.ICON_PATH);
                foreach (string iconPath in iconPaths)
                {
                    Bitmap icon = new Bitmap(iconPath);
                    icons.AddInputImage(icon);
                }
            }

            Dictionary<int, Bitmap> iconImagesBySize = icons
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
                files[appName + "/Assets.xcassets/AppIcon.appiconset/icon" + size + ".png"] = new FileOutput()
                {
                    Type = FileOutputType.Image,
                    Bitmap = iconImagesBySize[size],
                };
            }

        }

        public void ExportProjectOLD(
            Dictionary<string, FileOutput> output,
            string byteCode,
            IList<LibraryForExport> libraries,
            Build.ResourceDatabase resourceDatabase,
            Options options)
        {
            options.SetOption(ExportOptionKey.JS_FILE_PREFIX, null);
            options.SetOption(ExportOptionKey.JS_FULL_PAGE, false); // iOS export has its own enforced fullscreen logic
            options.SetOption(ExportOptionKey.JS_HEAD_EXTRAS, "");
            Dictionary<string, string> replacements = this.GenerateReplacementDictionary(options, resourceDatabase);

            Dictionary<string, FileOutput> files = new Dictionary<string, FileOutput>();
            Dictionary<string, FileOutput> basicProject = new Dictionary<string, FileOutput>();
            this.ParentPlatform.ExportProject(
                basicProject,
                byteCode,
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
                    Bitmap = new Bitmap(options.GetString(ExportOptionKey.LAUNCHSCREEN_PATH)),
                };
            }
            else
            {
                string resourcePath = "SwiftResources/" + (useLandscapeForLaunchscreen ? "launchhorizontal.png" : "launchvertical.png");
                byte[] bytes = new ResourceStore(typeof(JavaScriptAppIos.PlatformImpl)).ReadAssemblyFileBytes(resourcePath);

                launchScreen = new FileOutput()
                {
                    Type = FileOutputType.Image,
                    Bitmap = new Bitmap(bytes),
                };
            }
            files["%%%PROJECT_ID%%%/%%%PROJECT_ID%%%/Assets.xcassets/Launchscreen.imageset/launchscreen.png"] = launchScreen;
            replacements["LAUNCH_SCREEN_WIDTH"] = launchScreen.Bitmap.Width.ToString();
            replacements["LAUNCH_SCREEN_HEIGHT"] = launchScreen.Bitmap.Height.ToString();

            IconSetGenerator icons = new IconSetGenerator();
            if (options.GetBool(ExportOptionKey.HAS_ICON))
            {
                string[] iconPaths = options.GetStringArray(ExportOptionKey.ICON_PATH);
                foreach (string iconPath in iconPaths)
                {
                    Bitmap icon = new Bitmap(iconPath);
                    icons.AddInputImage(icon);
                }
            }

            Dictionary<int, Bitmap> iconImagesBySize = icons
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
            IList<LibraryForExport> everyLibrary)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<string, string> GenerateReplacementDictionary(
            Options options,
            Build.ResourceDatabase resDb)
        {
            Dictionary<string, string> replacements = this.ParentPlatform.GenerateReplacementDictionary(options, resDb);
            replacements["ORGANIZATION_NAME"] = options.GetStringOrNull(ExportOptionKey.ORGANIZATION_NAME) ?? "Organization Name";
            replacements["ORGANIZATION_NAME_LOWERCASE"] = replacements["ORGANIZATION_NAME"].Replace(" ", "").ToLowerInvariant();

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
