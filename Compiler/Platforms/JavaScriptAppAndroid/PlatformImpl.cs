using Common;
using Pastel.Nodes;
using Platform;
using System;
using System.Collections.Generic;

namespace JavaScriptAppAndroid
{
    public class PlatformImpl : AbstractPlatform
    {
        public override string InheritsFrom { get { return "javascript-app"; } }
        public override string Name { get { return "javascript-app-android"; } }
        public override string NL { get { return "\n"; } }

        public PlatformImpl()
        {
            this.ContextFreePlatformImpl = new ContextFreeJavaScriptAppAndroidPlatform();
        }

        public override void ExportProject(
            Dictionary<string, FileOutput> output,
            IList<VariableDeclaration> globals,
            IList<StructDefinition> structDefinitions,
            IList<FunctionDefinition> functionDefinitions,
            IList<LibraryForExport> libraries,
            ResourceDatabase resourceDatabase,
            Options options,
            ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform)
        {
            Dictionary<string, string> replacements = this.GenerateReplacementDictionary(options, resourceDatabase);
            this.OutputAndroidBoilerplate(output, replacements, options);

            options.SetOption(ExportOptionKey.JS_FILE_PREFIX, null);
            options.SetOption(ExportOptionKey.JS_FULL_PAGE, false); // TODO: figure out if logic should be implemented in the web view for this.
            options.SetOption(ExportOptionKey.JS_HEAD_EXTRAS, string.Join(
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
            ));

            Dictionary<string, FileOutput> files = new Dictionary<string, FileOutput>();
            Dictionary<string, FileOutput> basicProject = new Dictionary<string, FileOutput>();
            this.ParentPlatform.ExportProject(
                basicProject,
                globals,
                structDefinitions,
                functionDefinitions,
                libraries,
                resourceDatabase,
                options,
                libraryNativeInvocationTranslatorProviderForPlatform);

            // TODO: not good. The library inclusions should automatically be populated in LangJavaScript platforms.
            // This is also done identically in the ChromeApp PlatformImpl.
            replacements["JS_LIB_INCLUSIONS"] = JavaScriptApp.PlatformImpl.GenerateJsLibInclusionHtml(basicProject.Keys);

            foreach (string filePath in basicProject.Keys)
            {
                FileOutput file = basicProject[filePath];
                if (filePath.EndsWith("index.html"))
                {
                    file.TextContent = file.TextContent.Replace(
                        "<script type=\"text / javascript\" src=\"",
                        "<script type=\"text / javascript\" src=\"file:///android_asset/");
                }
                files["app/src/main/assets/" + filePath] = file;
            }

            // TODO: use orientations
            OrientationParser orientations = new OrientationParser(options);

            foreach (string filename in files.Keys)
            {
                output[this.ApplyReplacements(filename, replacements)] = files[filename];
            }
        }

        public override Dictionary<string, string> GenerateReplacementDictionary(Options options, ResourceDatabase resDb)
        {
            Dictionary<string, string> replacements = this.ParentPlatform.GenerateReplacementDictionary(options, resDb);

            // This logic is duplicated in LangJava's PlatformImpl
            replacements["JAVA_PACKAGE"] = (options.GetStringOrNull(ExportOptionKey.JAVA_PACKAGE) ?? options.GetString(ExportOptionKey.PROJECT_ID)).ToLower();
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

        public override void ExportStandaloneVm(Dictionary<string, FileOutput> output, IList<VariableDeclaration> globals, IList<StructDefinition> structDefinitions, IList<FunctionDefinition> functionDefinitions, IList<LibraryForExport> everyLibrary, ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform) { throw new NotImplementedException(); }
        public override string GenerateCodeForFunction(AbstractTranslator translator, FunctionDefinition funcDef) { throw new NotImplementedException(); }
        public override string GenerateCodeForGlobalsDefinitions(AbstractTranslator translator, IList<VariableDeclaration> globals) { throw new NotImplementedException(); }
        public override string GenerateCodeForStruct(AbstractTranslator translator, StructDefinition structDef) { throw new NotImplementedException(); }

        public void OutputAndroidBoilerplate(Dictionary<string, FileOutput> output, Dictionary<string, string> replacements, Options options)
        {
            string packagedDir = replacements["JAVA_PACKAGE"].Replace('.', '/');
            output[".gitignore"] = this.LoadTextFile("JsAndroidResources/gitignore.txt", replacements);
            output["build.gradle"] = this.LoadTextFile("JsAndroidResources/buildGradle.txt", replacements);
            output["android.iml"] = this.LoadTextFile("JsAndroidResources/androidIml.txt", replacements);
            output["gradle.properties"] = this.LoadTextFile("JsAndroidResources/gradleProperties.txt", replacements);
            output["gradlew"] = this.LoadTextFile("JsAndroidResources/gradlew.txt", replacements);
            output["gradlew.bat"] = this.LoadTextFile("JsAndroidResources/gradlewBat.txt", replacements);
            output["local.properties"] = this.LoadTextFile("JsAndroidResources/localProperties.txt", replacements);
            output["settings.gradle"] = this.LoadTextFile("JsAndroidResources/settingsGradle.txt", replacements);

            output["gradle/wrapper/gradle-wrapper.jar"] = this.LoadBinaryFile("JsAndroidResources/gradle/wrapper/GradleWrapper.jar");
            output["gradle/wrapper/gradle-wrapper.properties"] = this.LoadTextFile("JsAndroidResources/gradle/wrapper/GradleWrapperProperties.txt", replacements);

            output[".idea/.name"] = this.LoadTextFile("JsAndroidResources/idea/name.txt", replacements);
            output[".idea/compiler.xml"] = this.LoadTextFile("JsAndroidResources/idea/compilerXml.txt", replacements);
            output[".idea/encodings.xml"] = this.LoadTextFile("JsAndroidResources/idea/encodingsXml.txt", replacements);
            output[".idea/gradle.xml"] = this.LoadTextFile("JsAndroidResources/idea/gradleXml.txt", replacements);
            output[".idea/misc.xml"] = this.LoadTextFile("JsAndroidResources/idea/miscXml.txt", replacements);
            output[".idea/modules.xml"] = this.LoadTextFile("JsAndroidResources/idea/modulesXml.txt", replacements);
            output[".idea/vcs.xml"] = this.LoadTextFile("JsAndroidResources/idea/vcsXml.txt", replacements);
            output[".idea/copyright/profiles_settings.xml"] = this.LoadTextFile("JsAndroidResources/idea/copyright/profileSettings.txt", replacements);
            output[".idea/scopes/scope_settings.xml"] = this.LoadTextFile("JsAndroidResources/idea/scopes/scopeSettings.txt", replacements);

            if (!options.GetBool(ExportOptionKey.ANDROID_SKIP_WORKSPACE_XML))
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
            if (options.GetBool(ExportOptionKey.HAS_ICON))
            {
                string iconPath = options.GetString(ExportOptionKey.ICON_PATH);
                SystemBitmap icon = new SystemBitmap(iconPath);
                icons.AddInputImage(icon);
            }
            Dictionary<int, SystemBitmap> iconImagesBySize = icons
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

        public override void GleanInformationFromPreviouslyExportedProject(Options options, string outputDirectory)
        {
            bool skipWorkspaceXml = FileUtil.FileExists(outputDirectory + "/.idea/workspace.xml");
            options.SetOption(ExportOptionKey.ANDROID_SKIP_WORKSPACE_XML, true);
        }
    }
}
