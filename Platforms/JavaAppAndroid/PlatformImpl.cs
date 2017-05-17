using System;
using System.Collections.Generic;
using System.Text;
using Common;
using Pastel.Nodes;
using Platform;

namespace JavaAppAndroid
{
    public class PlatformImpl : Platform.AbstractPlatform
    {
        public PlatformImpl() : base()
        {
            this.Translator = new JavaAppAndroidTranslator(this);
        }

        public override string InheritsFrom { get { return "lang-java"; } }
        public override string Name { get { return "java-app-android"; } }
        public override string NL { get { return "\n"; } }

        public override Dictionary<string, FileOutput> ExportProject(IList<VariableDeclaration> globals, IList<StructDefinition> structDefinitions, IList<FunctionDefinition> functionDefinitions, IList<LibraryForExport> libraries, ResourceDatabase resourceDatabase, Options options, ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform)
        {
            Dictionary<string, FileOutput> output = new Dictionary<string, FileOutput>();
            Dictionary<string, string> replacements = this.GenerateReplacementDictionary(options, resourceDatabase);
            this.OutputAndroidBoilerplate(output, replacements);


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
            this.CopyResourceAsText(output, "app/src/main/java/org/crayonlang/interpreter/LibraryInstance.java", "Resources/LibraryInstance.txt", replacements);
            this.CopyResourceAsText(output, "app/src/main/java/org/crayonlang/interpreter/LibraryLoader.java", "Resources/LibraryLoader.txt", replacements);

            return output;
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

        private void OutputAndroidBoilerplate(Dictionary<string, FileOutput> output, Dictionary<string, string> replacements)
        {
            output[".gitignore"] = this.LoadTextFile("Resources/gitignore.txt", replacements);
            output["build.gradle"] = this.LoadTextFile("Resources/buildGradle.txt", replacements);
            output["CrayonSampleApp.iml"] = this.LoadTextFile("Resources/CrayonAppIml.txt", replacements);
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
            output[".idea/workspace.xml"] = this.LoadTextFile("Resources/idea/workspaceXml.txt", replacements);
            output[".idea/copyright/profiles_settings.xml"] = this.LoadTextFile("Resources/idea/copyright/profileSettings.txt", replacements);
            output[".idea/scopes/scope_settings.xml"] = this.LoadTextFile("Resources/idea/scopes/scopeSettings.txt", replacements);

            output["app/.gitignore"] = this.LoadTextFile("Resources/app/gitignore.txt", replacements);
            output["app/app.iml"] = this.LoadTextFile("Resources/app/appIml.txt", replacements);
            output["app/build.gradle"] = this.LoadTextFile("Resources/app/buildGradle.txt", replacements);
            output["app/proguard-rules.txt"] = this.LoadTextFile("Resources/app/proguardRules.txt", replacements);
            output["app/src/main/AndroidManifest.xml"] = this.LoadTextFile("Resources/app/src/main/AndroidManifestXml.txt", replacements);
            output["app/src/main/ic_launcher-web.png"] = this.LoadBinaryFile("Resources/app/src/main/IcLauncherWeb.png");

            output["app/src/main/java/org/crayonlang/crayonsampleapp/app/MainActivity.java"] = this.LoadTextFile("Resources/app/src/main/java/org/crayonlang/sampleapp/app/MainActivityJava.txt", replacements);
            output["app/src/main/java/org/crayonlang/crayonsampleapp/app/DrawableView.java"] = this.LoadTextFile("Resources/app/src/main/java/org/crayonlang/sampleapp/app/DrawableViewJava.txt", replacements);

            output["app/src/main/res/drawable-hdpi/ic_launcher.png"] = this.LoadBinaryFile("Resources/app/src/main/res/drawableHdpi/ic_launcher.png");
            output["app/src/main/res/drawable-mdpi/ic_launcher.png"] = this.LoadBinaryFile("Resources/app/src/main/res/drawableMdpi/ic_launcher.png");
            output["app/src/main/res/drawable-xhdpi/ic_launcher.png"] = this.LoadBinaryFile("Resources/app/src/main/res/drawableXhdpi/ic_launcher.png");
            output["app/src/main/res/drawable-xxhdpi/ic_launcher.png"] = this.LoadBinaryFile("Resources/app/src/main/res/drawableXxhdpi/ic_launcher.png");
            output["app/src/main/res/layout/activity_main.xml"] = this.LoadTextFile("Resources/app/src/main/res/layout/ActivityMainXml.txt", replacements);
            output["app/src/main/res/menu/main.xml"] = this.LoadTextFile("Resources/app/src/main/res/menu/MainXml.txt", replacements);
            output["app/src/main/res/values/dimens.xml"] = this.LoadTextFile("Resources/app/src/main/res/values/DimensXml.txt", replacements);
            output["app/src/main/res/values/strings.xml"] = this.LoadTextFile("Resources/app/src/main/res/values/StringsXml.txt", replacements);
            output["app/src/main/res/values/styles.xml"] = this.LoadTextFile("Resources/app/src/main/res/values/StylesXml.txt", replacements);
            output["app/src/main/res/values-w820dp/dimens.xml"] = this.LoadTextFile("Resources/app/src/main/res/valuesW820dp/DimensXml.txt", replacements);
        }

        public override Dictionary<string, FileOutput> ExportStandaloneVm(IList<VariableDeclaration> globals, IList<StructDefinition> structDefinitions, IList<FunctionDefinition> functionDefinitions, IList<LibraryForExport> everyLibrary, ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform)
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
    }
}
