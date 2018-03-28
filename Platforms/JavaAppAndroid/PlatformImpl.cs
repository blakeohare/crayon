using Common;
using Pastel.Nodes;
using Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JavaAppAndroid
{
    public class PlatformImpl : AbstractPlatform
    {
        public PlatformImpl() : base()
        {
            this.Translator = new JavaAppAndroidTranslator(this);
        }

        public override string InheritsFrom { get { return "lang-java"; } }
        public override string Name { get { return "java-app-android"; } }
        public override string NL { get { return "\n"; } }

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

            string srcPath = "app/src/main/java";

            string[] imports = new string[]
            {
                "import org.crayonlang.interpreter.AndroidTranslationHelper;",
            };

            LangJava.PlatformImpl.ExportJavaLibraries(this, srcPath, libraries, output, libraryNativeInvocationTranslatorProviderForPlatform, imports);

            foreach (StructDefinition structDef in structDefinitions)
            {
                output["app/src/main/java/org/crayonlang/interpreter/structs/" + structDef.NameToken.Value + ".java"] = new FileOutput()
                {
                    Type = FileOutputType.Text,
                    TextContent = this.GenerateCodeForStruct(structDef),
                };
            }

            StringBuilder sb = new StringBuilder();

            sb.Append(string.Join(this.NL, new string[] {
                "package org.crayonlang.interpreter;",
                "",
                "import java.util.ArrayList;",
                "import java.util.HashMap;",
                "import org.crayonlang.interpreter.structs.*;",
                "",
                "public final class Interpreter {",
                "  private Interpreter() {}",
                "",
            }));

            foreach (FunctionDefinition fnDef in functionDefinitions)
            {
                this.Translator.TabDepth = 1;
                sb.Append(this.GenerateCodeForFunction(this.Translator, fnDef));
                sb.Append(this.NL);
            }
            this.Translator.TabDepth = 0;
            sb.Append("}");
            sb.Append(this.NL);

            output["app/src/main/java/org/crayonlang/interpreter/Interpreter.java"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = sb.ToString(),
            };

            output["app/src/main/java/org/crayonlang/interpreter/VmGlobal.java"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = this.GenerateCodeForGlobalsDefinitions(this.Translator, globals),
            };

            // common Java helper files
            this.CopyResourceAsText(output, "app/src/main/java/org/crayonlang/interpreter/TranslationHelper.java", "Resources/TranslationHelper.txt", replacements);
            this.CopyResourceAsText(output, "app/src/main/java/org/crayonlang/interpreter/FastList.java", "Resources/FastList.txt", replacements);
            this.CopyResourceAsText(output, "app/src/main/java/org/crayonlang/interpreter/AndroidTranslationHelper.java", "Resources/app/src/main/java/org/crayonlang/interpreter/AndroidTranslationHelper.txt", replacements);
            this.CopyResourceAsText(output, "app/src/main/java/org/crayonlang/interpreter/LibraryFunctionPointer.java", "Resources/LibraryFunctionPointer.txt", replacements);
            this.CopyResourceAsText(output, "app/src/main/java/org/crayonlang/interpreter/LibraryInstance.java", "Resources/LibraryInstance.txt", replacements);
            this.CopyResourceAsText(output, "app/src/main/java/org/crayonlang/interpreter/LibraryLoader.java", "Resources/LibraryLoader.txt", replacements);

            output["app/src/main/assets/bytecode.txt"] = resourceDatabase.ByteCodeFile;
            output["app/src/main/assets/resourcemanifest.txt"] = resourceDatabase.ResourceManifestFile;

            output["app/src/main/assets/imagesheetmanifest.txt"] =
                resourceDatabase.ImageSheetManifestFile != null
                    ? resourceDatabase.ImageSheetManifestFile
                    : new FileOutput() { Type = FileOutputType.Text, TextContent = "" };

            foreach (string imageResourceFile in resourceDatabase.ImageSheetFiles.Keys)
            {
                FileOutput file = resourceDatabase.ImageSheetFiles[imageResourceFile];
                output["app/src/main/assets/images/" + imageResourceFile] = file;
            }
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

        public void OutputAndroidBoilerplate(Dictionary<string, FileOutput> output, Dictionary<string, string> replacements, Options options)
        {
            string packagedDir = replacements["JAVA_PACKAGE"].Replace('.', '/');
            output[".gitignore"] = this.LoadTextFile("Resources/gitignore.txt", replacements);
            output["build.gradle"] = this.LoadTextFile("Resources/buildGradle.txt", replacements);
            output["android.iml"] = this.LoadTextFile("Resources/androidIml.txt", replacements);
            output["gradle.properties"] = this.LoadTextFile("Resources/gradleProperties.txt", replacements);
            output["gradlew"] = this.LoadTextFile("Resources/gradlew.txt", replacements);
            output["gradlew.bat"] = this.LoadTextFile("Resources/gradlewBat.txt", replacements);
            output["local.properties"] = this.LoadTextFile("Resources/localProperties.txt", replacements);
            output["settings.gradle"] = this.LoadTextFile("Resources/settingsGradle.txt", replacements);

            output["gradle/wrapper/gradle-wrapper.jar"] = this.LoadBinaryFile("Resources/gradle/wrapper/GradleWrapper.jar");
            output["gradle/wrapper/gradle-wrapper.properties"] = this.LoadTextFile("Resources/gradle/wrapper/GradleWrapperProperties.txt", replacements);

            output[".idea/.name"] = this.LoadTextFile("Resources/idea/name.txt", replacements);
            output[".idea/compiler.xml"] = this.LoadTextFile("Resources/idea/compilerXml.txt", replacements);
            output[".idea/encodings.xml"] = this.LoadTextFile("Resources/idea/encodingsXml.txt", replacements);
            output[".idea/gradle.xml"] = this.LoadTextFile("Resources/idea/gradleXml.txt", replacements);
            output[".idea/misc.xml"] = this.LoadTextFile("Resources/idea/miscXml.txt", replacements);
            output[".idea/modules.xml"] = this.LoadTextFile("Resources/idea/modulesXml.txt", replacements);
            output[".idea/vcs.xml"] = this.LoadTextFile("Resources/idea/vcsXml.txt", replacements);
            output[".idea/copyright/profiles_settings.xml"] = this.LoadTextFile("Resources/idea/copyright/profileSettings.txt", replacements);
            output[".idea/scopes/scope_settings.xml"] = this.LoadTextFile("Resources/idea/scopes/scopeSettings.txt", replacements);

            if (!options.GetBool(ExportOptionKey.ANDROID_SKIP_WORKSPACE_XML))
            {
                output[".idea/workspace.xml"] = this.LoadTextFile("Resources/idea/workspaceXml.txt", replacements);
            }

            output["app/.gitignore"] = this.LoadTextFile("Resources/app/gitignore.txt", replacements);
            output["app/app.iml"] = this.LoadTextFile("Resources/app/appIml.txt", replacements);
            output["app/build.gradle"] = this.LoadTextFile("Resources/app/buildGradle.txt", replacements);
            output["app/proguard-rules.txt"] = this.LoadTextFile("Resources/app/proguardRules.txt", replacements);

            output["app/src/main/java/" + packagedDir + "/app/MainActivity.java"] = this.LoadTextFile("Resources/app/src/main/java/org/crayonlang/sampleapp/app/MainActivityJava.txt", replacements);
            output["app/src/main/res/layout/activity_main.xml"] = this.LoadTextFile("Resources/app/src/main/res/layout/ActivityMainXml.txt", replacements);
            output["app/src/main/res/menu/main.xml"] = this.LoadTextFile("Resources/app/src/main/res/menu/MainXml.txt", replacements);
            output["app/src/main/res/values/dimens.xml"] = this.LoadTextFile("Resources/app/src/main/res/values/DimensXml.txt", replacements);
            output["app/src/main/res/values/strings.xml"] = this.LoadTextFile("Resources/app/src/main/res/values/StringsXml.txt", replacements);
            output["app/src/main/res/values/styles.xml"] = this.LoadTextFile("Resources/app/src/main/res/values/StylesXml.txt", replacements);
            output["app/src/main/res/values-w820dp/dimens.xml"] = this.LoadTextFile("Resources/app/src/main/res/valuesW820dp/DimensXml.txt", replacements);

            FileOutput androidManifest = this.LoadTextFile("Resources/app/src/main/AndroidManifestXml.txt", replacements);
            output["app/src/main/AndroidManifest.xml"] = androidManifest;
            androidManifest.TextContent = this.AdjustAndroidManifest(androidManifest.TextContent, options);

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
        }

        public override void ExportStandaloneVm(Dictionary<string, FileOutput> output, IList<VariableDeclaration> globals, IList<StructDefinition> structDefinitions, IList<FunctionDefinition> functionDefinitions, IList<LibraryForExport> everyLibrary, ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform)
        {
            throw new NotImplementedException();
        }

        public override string GenerateCodeForFunction(AbstractTranslator translator, FunctionDefinition funcDef)
        {
            return this.ParentPlatform.GenerateCodeForFunction(translator, funcDef);
        }

        public override string GenerateCodeForGlobalsDefinitions(AbstractTranslator translator, IList<VariableDeclaration> globals)
        {
            return this.ParentPlatform.GenerateCodeForGlobalsDefinitions(translator, globals);
        }

        public override string GenerateCodeForStruct(StructDefinition structDef)
        {
            return this.ParentPlatform.GenerateCodeForStruct(structDef);
        }

        public override Dictionary<string, string> GenerateReplacementDictionary(Options options, ResourceDatabase resDb)
        {
            return this.ParentPlatform.GenerateReplacementDictionary(options, resDb);
        }

        public override IDictionary<string, object> GetConstantFlags()
        {
            return new Dictionary<string, object>()
            {
                {  "IS_JAVA", true },
            };
        }

        private string AdjustAndroidManifest(string content, Options options)
        {
            // TODO: LibraryForExport should maybe have an UsesOpenGL flag?
            bool usesOpenGl = options.GetArray(ExportOptionKey.LIBRARIES_USED)
                .Cast<LibraryForExport>()
                .Where(lfe => lfe.Name == "Game")
                .Count() > 0;

            // This is kind of a weird way to do this...

            string[] parts = content.Split(new string[] { "###" }, StringSplitOptions.None);
            Dictionary<string, int> flags = new Dictionary<string, int>();
            for (int i = 0; i < parts.Length; ++i)
            {
                flags[parts[i]] = i;
            }

            parts[flags["USES_OPENGL_BEGIN"]] = "";
            parts[flags["USES_OPENGL_END"]] = "";

            if (!usesOpenGl)
            {
                parts[flags["USES_OPENGL_BEGIN"] + 1] = "";
            }

            string[] lines = string.Join("", parts)
                .Split('\n')
                .Select(line => line.TrimEnd())
                .Where(line => line.Length > 0)
                .ToArray();
            return string.Join("\n", lines);
        }

        public override void GleanInformationFromPreviouslyExportedProject(Options options, string outputDirectory)
        {
            bool skipWorkspaceXml = FileUtil.FileExists(outputDirectory + "/.idea/workspace.xml");
            options.SetOption(ExportOptionKey.ANDROID_SKIP_WORKSPACE_XML, true);
        }
    }
}
