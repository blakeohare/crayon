using Common;
using CommonUtil.Disk;
using CommonUtil.Images;
using Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using Wax;

namespace JavaScriptAppAndroid
{
    public class PlatformImpl : AbstractPlatform
    {
        public override string InheritsFrom { get { return "javascript-app"; } }
        public override string Name { get { return "javascript-app-android"; } }
        public override string NL { get { return "\n"; } }

        public PlatformImpl()
            : base("JAVASCRIPT")
        { }

        public override void ExportProject(
            Dictionary<string, FileOutput> output,
            BuildData buildData,
            ExportProperties exportProperties)
        {
            Dictionary<string, string> replacements = this.GenerateReplacementDictionary(exportProperties, buildData);
            this.OutputAndroidBoilerplate(output, replacements, exportProperties);

            exportProperties.JsFilePrefix = null;
            exportProperties.JsFullPage = false;
            exportProperties.JsHeadExtras = string.Join(
                "\n",
                "<script type=\"text/javascript\" src=\"android.js\"></script>",
                "<style type=\"text/css\">",
                "  body { margin:0px; background-color:#000; }",
                "  #crayon_host {",
                "    background-color:#000;",
                "    text-align:left;",
                "    width:100%;",
                "    height:100%;",
                "  }",
                "</style>"
            );

            Dictionary<string, FileOutput> files = new Dictionary<string, FileOutput>();
            Dictionary<string, FileOutput> basicProject = new Dictionary<string, FileOutput>();
            this.ParentPlatform.ExportProject(
                basicProject,
                buildData,
                exportProperties);

            foreach (string filePath in basicProject.Keys)
            {
                FileOutput file = basicProject[filePath];
                if (filePath.EndsWith("index.html"))
                {
                    file.TextContent = file.TextContent.Replace(
                        "<script type=\"text/javascript\" src=\"",
                        "<script type=\"text/javascript\" src=\"file:///android_asset/");

                    file.TextContent = file.TextContent.Replace(
                        "<link rel=\"shortcut icon\" href=\"favicon.ico\">",
                        "");
                }
                else if (filePath == "test_server.py")
                {
                    // TODO: add an option called INCLUDE_TEST_SERVER for the parent platform so that this doesn't appear in the first place.
                    continue;
                }
                files["app/src/main/assets/" + filePath] = file;
            }

            // TODO: use orientations
            Orientations orientations = exportProperties.Orientations;

            foreach (string filename in files.Keys)
            {
                output[this.ApplyReplacements(filename, replacements)] = files[filename];
            }
        }

        private string ConvertOrientationString(Orientations orientations)
        {
            bool portrait = orientations.SupportsPortrait;
            bool invPortrait = orientations.SupportsUpsideDown;
            bool left = orientations.SupportsLandscapeLeft;
            bool right = orientations.SupportsLandscapeRight;

            bool vert = portrait && invPortrait;
            bool hor = left && right;

            if (!vert && !hor) return "unspecified";
            if (hor && vert) return "unspecified";

            if (!hor)
            {
                if (vert) return "sensorPortrait";
                if (portrait) return "portrait";
                return "reversePortrait";
            }

            if (!vert)
            {
                if (hor) return "sensorLandscape";
                if (left) return "landscape";
                return "reverseLandscape";
            }
            return "unspecified";
        }

        public override Dictionary<string, string> GenerateReplacementDictionary(
            ExportProperties exportProperties,
            BuildData buildData)
        {
            Dictionary<string, string> replacements = this.ParentPlatform.GenerateReplacementDictionary(exportProperties, buildData);

            replacements["ANDROID_ORIENTATION"] = this.ConvertOrientationString(exportProperties.Orientations);

            // This logic is duplicated in LangJava's PlatformImpl
            replacements["JAVA_PACKAGE"] = (exportProperties.JavaPackage ?? exportProperties.ProjectID).ToLowerInvariant();
            if (replacements["JAVA_PACKAGE"].StartsWith("org.crayonlang.interpreter"))
            {
                throw new InvalidOperationException("Cannot use org.crayonlang.interpreter as the project's package.");
            }

            return replacements;
        }

        public override IDictionary<string, object> GetConstantFlags()
        {
            return this.ParentPlatform.GetConstantFlags();
        }

        private string EscapeAndroidSdkPath(string original)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (char c in original)
            {
                switch (c)
                {
                    case ':': sb.Append("\\:"); break;
                    case '\\': sb.Append('/'); break;
                    default: sb.Append(c); break;
                }
            }
            return sb.ToString();
        }

        public void OutputAndroidBoilerplate(Dictionary<string, FileOutput> output, Dictionary<string, string> replacements, ExportProperties exportProperties)
        {
            string androidSdkLocation = exportProperties.AndroidSdkLocation;
            replacements["ANDROID_SDK_LOCATION"] = this.EscapeAndroidSdkPath(androidSdkLocation);

            string packagedDir = replacements["JAVA_PACKAGE"].Replace('.', '/');
            output[".gitignore"] = this.LoadTextFile("JsAndroidResources/gitignore.txt", replacements);
            output["build.gradle"] = this.LoadTextFile("JsAndroidResources/buildGradle.txt", replacements);
            output[replacements["PROJECT_ID_LOWERCASE"] + ".iml"] = this.LoadTextFile("JsAndroidResources/androidIml.txt", replacements);
            output["gradle.properties"] = this.LoadTextFile("JsAndroidResources/gradleProperties.txt", replacements);
            output["gradlew"] = this.LoadTextFile("JsAndroidResources/gradlew.txt", replacements);
            output["gradlew.bat"] = this.LoadTextFile("JsAndroidResources/gradlewBat.txt", replacements);
            output["local.properties"] = this.LoadTextFile("JsAndroidResources/localProperties.txt", replacements);
            output["settings.gradle"] = this.LoadTextFile("JsAndroidResources/settingsGradle.txt", replacements);

            output["gradle/wrapper/gradle-wrapper.jar"] = this.LoadBinaryFile("JsAndroidResources/gradle/wrapper/GradleWrapper.jar");
            output["gradle/wrapper/gradle-wrapper.properties"] = this.LoadTextFile("JsAndroidResources/gradle/wrapper/GradleWrapperProperties.txt", replacements);

            output[".idea/compiler.xml"] = this.LoadTextFile("JsAndroidResources/idea/compilerXml.txt", replacements);
            output[".idea/encodings.xml"] = this.LoadTextFile("JsAndroidResources/idea/encodingsXml.txt", replacements);
            output[".idea/gradle.xml"] = this.LoadTextFile("JsAndroidResources/idea/gradleXml.txt", replacements);
            output[".idea/misc.xml"] = this.LoadTextFile("JsAndroidResources/idea/miscXml.txt", replacements);
            output[".idea/modules.xml"] = this.LoadTextFile("JsAndroidResources/idea/modulesXml.txt", replacements);
            output[".idea/vcs.xml"] = this.LoadTextFile("JsAndroidResources/idea/vcsXml.txt", replacements);
            output[".idea/copyright/profiles_settings.xml"] = this.LoadTextFile("JsAndroidResources/idea/copyright/profileSettings.txt", replacements);
            output[".idea/scopes/scope_settings.xml"] = this.LoadTextFile("JsAndroidResources/idea/scopes/scopeSettings.txt", replacements);

            if (!exportProperties.SkipAndroidWorkspaceXml)
            {
                output[".idea/workspace.xml"] = this.LoadTextFile("JsAndroidResources/idea/workspaceXml.txt", replacements);
            }

            output["app/.gitignore"] = this.LoadTextFile("JsAndroidResources/app/gitignore.txt", replacements);
            output["app/app.iml"] = this.LoadTextFile("JsAndroidResources/app/appIml.txt", replacements);
            output["app/build.gradle"] = this.LoadTextFile("JsAndroidResources/app/buildGradle.txt", replacements);
            output["app/proguard-rules.txt"] = this.LoadTextFile("JsAndroidResources/app/proguardRules.txt", replacements);

            output["app/src/main/java/" + packagedDir + "/app/MainActivity.java"] = this.LoadTextFile("JsAndroidResources/app/src/main/java/org/crayonlang/sampleapp/app/MainActivityJava.txt", replacements);
            output["app/src/main/java/" + packagedDir + "/app/CrayonWebView.java"] = this.LoadTextFile("JsAndroidResources/app/src/main/java/org/crayonlang/sampleapp/app/CrayonWebViewJava.txt", replacements);
            output["app/src/main/res/layout/activity_main.xml"] = this.LoadTextFile("JsAndroidResources/app/src/main/res/layout/ActivityMainXml.txt", replacements);
            output["app/src/main/res/menu/main.xml"] = this.LoadTextFile("JsAndroidResources/app/src/main/res/menu/MainXml.txt", replacements);
            output["app/src/main/res/values/dimens.xml"] = this.LoadTextFile("JsAndroidResources/app/src/main/res/values/DimensXml.txt", replacements);
            output["app/src/main/res/values/strings.xml"] = this.LoadTextFile("JsAndroidResources/app/src/main/res/values/StringsXml.txt", replacements);
            output["app/src/main/res/values/styles.xml"] = this.LoadTextFile("JsAndroidResources/app/src/main/res/values/StylesXml.txt", replacements);
            output["app/src/main/res/values-w820dp/dimens.xml"] = this.LoadTextFile("JsAndroidResources/app/src/main/res/valuesW820dp/DimensXml.txt", replacements);

            FileOutput androidManifest = this.LoadTextFile("JsAndroidResources/app/src/main/AndroidManifestXml.txt", replacements);
            output["app/src/main/AndroidManifest.xml"] = androidManifest;

            IconSetGenerator icons = new IconSetGenerator();
            if (exportProperties.HasIcon)
            {
                string[] iconPaths = exportProperties.IconPaths;
                foreach (string iconPath in iconPaths)
                {
                    Bitmap icon = new Bitmap(iconPath);
                    icons.AddInputImage(icon);
                }
            }
            Dictionary<int, Bitmap> iconImagesBySize = icons
                .AddOutputSize(48)
                .AddOutputSize(72)
                .AddOutputSize(96)
                .AddOutputSize(144)
                .AddOutputSize(512)
                .GenerateWithDefaultFallback();

            output["app/src/main/res/drawable-mdpi/ic_launcher.png"] = new FileOutput() { Type = FileOutputType.Image, Bitmap = iconImagesBySize[48] };
            output["app/src/main/res/drawable-hdpi/ic_launcher.png"] = new FileOutput() { Type = FileOutputType.Image, Bitmap = iconImagesBySize[72] };
            output["app/src/main/res/drawable-xhdpi/ic_launcher.png"] = new FileOutput() { Type = FileOutputType.Image, Bitmap = iconImagesBySize[96] };
            output["app/src/main/res/drawable-xxhdpi/ic_launcher.png"] = new FileOutput() { Type = FileOutputType.Image, Bitmap = iconImagesBySize[144] };
            output["app/src/main/ic_launcher-web.png"] = new FileOutput() { Type = FileOutputType.Image, Bitmap = iconImagesBySize[512] };

            output["app/src/main/assets/android.js"] = this.LoadTextFile("JsAndroidResources/app/src/main/assets/androidJs.txt", replacements);
        }

        private FileOutput LoadTextFile(string path, Dictionary<string, string> replacements)
        {
            return new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = this.LoadTextResource(path, replacements),
                TrimBomIfPresent = true,
            };
        }

        private FileOutput LoadBinaryFile(string path)
        {
            return new FileOutput()
            {
                Type = FileOutputType.Binary,
                BinaryContent = this.LoadBinaryResource(path),
            };
        }

        public override void GleanInformationFromPreviouslyExportedProject(ExportProperties exportProperties, string outputDirectory)
        {
            bool skipWorkspaceXml = FileUtil.FileExists(outputDirectory + "/.idea/workspace.xml");
            exportProperties.SkipAndroidWorkspaceXml = skipWorkspaceXml;
            exportProperties.AndroidSdkLocation = this.GetAndroidSdkLocation(outputDirectory);
        }

        private string GetAndroidSdkLocation(string outputDirectory)
        {
            string localPropertiesFile = outputDirectory + "/local.properties";
            if (FileUtil.FileExists(localPropertiesFile))
            {
                string sdkDirLine = FileUtil.ReadFileText(localPropertiesFile)
                    .Split('\n')
                    .Where(line => line.StartsWith("sdk.dir="))
                    .Select(line => line.Trim())
                    .FirstOrDefault();

                if (sdkDirLine != null)
                {
                    int equalsIndex = sdkDirLine.IndexOf("=");
                    string sdkValue = sdkDirLine.Substring(equalsIndex + 1);
                    sdkValue = sdkValue.Replace("\\\\", "\\").Replace("\\:", ":");
                    return sdkValue;
                }
            }

            return CommonUtil.Environment.EnvironmentVariables.Get("CRAYON_ANDROID_SDK")
                ?? @"C:\Program Files (x86)\Android\android-studio\sdk";
        }
    }
}
