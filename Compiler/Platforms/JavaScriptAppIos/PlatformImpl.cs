﻿using Platform;
using System.Collections.Generic;
using Wax;
using Wax.Util.Disk;
using Wax.Util.Images;

namespace JavaScriptAppIos
{
    public class PlatformImpl : AbstractPlatform
    {
        public override string Name { get { return "javascript-app-ios"; } }
        public override string NL { get { return "\n"; } }

        public PlatformImpl()
            : base("JAVASCRIPT")
        { }

        public override void ExportProject(
            Dictionary<string, FileOutput> output,
            BuildData buildData,
            ExportProperties exportProperties)
        {
            exportProperties.JsFilePrefix = null;
            exportProperties.JsFullPage = false; // iOS export has its own enforced fullscreen logic
            exportProperties.JsHeadExtras = "";
            Dictionary<string, string> replacements = this.GenerateReplacementDictionary(exportProperties, buildData);

            Dictionary<string, FileOutput> files = new Dictionary<string, FileOutput>();
            Dictionary<string, FileOutput> basicProject = new Dictionary<string, FileOutput>();
            this.ParentPlatform.ExportProject(
                basicProject,
                buildData,
                exportProperties);

            foreach (string filePath in basicProject.Keys)
            {
                files["%%%PROJECT_ID%%%/%%%PROJECT_ID%%%/jsres/" + filePath] = basicProject[filePath];
            }

            // TODO: use this in the pbxproj file.
            string uuidSeed = exportProperties.GuidSeed;

            Orientations orientations = exportProperties.Orientations;
            bool useLandscapeForLaunchscreen = orientations.SupportsLandscapeLeft || orientations.SupportsLandscapeRight;
            FileOutput launchScreen;
            if (exportProperties.HasLaunchScreen)
            {
                launchScreen = new FileOutput()
                {
                    Type = FileOutputType.Image,
                    Bitmap = new Bitmap(exportProperties.LaunchScreenPath),
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
            if (exportProperties.HasIcon)
            {
                foreach (string iconPath in exportProperties.IconPaths)
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

        public override Dictionary<string, string> GenerateReplacementDictionary(
            ExportProperties exportProperties,
            BuildData buildData)
        {
            Dictionary<string, string> replacements = this.ParentPlatform.GenerateReplacementDictionary(exportProperties, buildData);
            replacements["ORGANIZATION_NAME"] = "Organization Name";

            replacements["DEVELOPMENT_TEAM_ALL_CAPS"] = "";
            replacements["DEVELOPMENT_TEAM_NOT_CAPS"] = "";
            string developmentTeam = exportProperties.IosDevTeamId;
            if (developmentTeam != null)
            {
                replacements["DEVELOPMENT_TEAM_NOT_CAPS"] = "DevelopmentTeam = " + developmentTeam + ";";
                replacements["DEVELOPMENT_TEAM_ALL_CAPS"] = "DEVELOPMENT_TEAM = " + developmentTeam + ";";
            }

            string bundleIdPrefix = exportProperties.IosBundlePrefix;
            replacements["IOS_BUNDLE_ID"] = bundleIdPrefix == null
                ? exportProperties.ProjectID
                : bundleIdPrefix + "." + exportProperties.ProjectID;

            Orientations orientations = exportProperties.Orientations;
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

        public override void GleanInformationFromPreviouslyExportedProject(ExportProperties exportProperties, string outputDirectory)
        {
            string pbxproj = FileUtil.JoinPath(
                outputDirectory,
                exportProperties.ProjectID ?? "",
                (exportProperties.ProjectID ?? "") + ".xcodeproj",
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
                            exportProperties.IosDevTeamId = value;
                        }
                    }
                }
            }
        }
    }
}
