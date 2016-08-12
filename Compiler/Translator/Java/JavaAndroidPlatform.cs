using Crayon.ParseTree;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Crayon.Translator.Java
{
    internal class JavaAndroidPlatform : JavaPlatform
    {
        public JavaAndroidPlatform()
            : base(PlatformId.JAVA_ANDROID, new JavaAndroidSystemFunctionTranslator(), true)
        { }

        public override string PlatformShortId { get { return "game-java-android"; } }

        public override Dictionary<string, FileOutput> Package(
            BuildContext buildContext,
            string projectId,
            Dictionary<string, ParseTree.Executable[]> finalCode,
            ICollection<ParseTree.StructDefinition> structDefinitions,
            string fileCopySourceRoot,
            ResourceDatabase resourceDatabase,
            SystemLibraryManager libraryManager)
        {
            Dictionary<string, FileOutput> output = new Dictionary<string, FileOutput>();
            string package = "com." + projectId.ToLowerInvariant() + ".app";
            string pathToJavaCode = "app/src/main/java/" + package.Replace('.', '/');

            Dictionary<string, string> replacements = new Dictionary<string, string>() {
                { "PROJECT_TITLE", projectId },
                { "PROJECT_ID", projectId },
                { "PACKAGE", package },
                { "CURRENT_YEAR", DateTime.Now.Year.ToString() },
                { "COPYRIGHT", "©" },
            };

            foreach (string simpleCopyText in new string[] {
                // Boilerplate Android project stuff
                "build.gradle|AndroidProject/Boilerplate/BuildGradle.txt",
                "gradle.properties|AndroidProject/Boilerplate/GradleProperties.txt",
                "gradlew|AndroidProject/Boilerplate/Gradlew.txt",
                "gradlew.bat|AndroidProject/Boilerplate/GradlewBat.txt",
                projectId + ".iml|AndroidProject/Boilerplate/ProjectIml.txt",
                "android.iml|AndroidProject/Boilerplate/AndroidIml.txt",
                "settings.gradle|AndroidProject/Boilerplate/SettingsGradle.txt",
                "gradle/wrapper/gradle-wrapper.jar|AndroidProject/Boilerplate/GradleWrapperJar",
                "gradle/wrapper/gradle-wrapper.properties|AndroidProject/Boilerplate/GradleWrapperProperties.txt",
                "app/app.iml|AndroidProject/Boilerplate/AppIml.txt",
                "app/build.gradle|AndroidProject/Boilerplate/AppBuildGradle.txt",
                "app/src/main/res/layout/activity_game.xml|AndroidProject/Boilerplate/ResLayoutActivityGameXml.txt",
                "app/src/main/res/values/attrs.xml|AndroidProject/Boilerplate/ResValuesAttrsXml.txt",
                "app/src/main/res/values/colors.xml|AndroidProject/Boilerplate/ResValuesColorsXml.txt",
                "app/src/main/res/values/strings.xml|AndroidProject/Boilerplate/ResValuesStringsXml.txt",
                "app/src/main/res/values/styles.xml|AndroidProject/Boilerplate/ResValuesStylesXml.txt",
                "app/src/main/res/values-v11/styles.xml|AndroidProject/Boilerplate/ResValuesV11StylesXml.txt",
                "app/src/main/res/drawable-hdpi/ic_launcher.png|AndroidProject/Boilerplate/ResDrawableHdpiIcLauncher.png",
                "app/src/main/res/drawable-mdpi/ic_launcher.png|AndroidProject/Boilerplate/ResDrawableMdpiIcLauncher.png",
                "app/src/main/res/drawable-xhdpi/ic_launcher.png|AndroidProject/Boilerplate/ResDrawableXhdpiIcLauncher.png",
                "app/src/main/res/drawable-xxhdpi/ic_launcher.png|AndroidProject/Boilerplate/ResDrawableXxhdpiIcLauncher.png",
                pathToJavaCode + "/util/SystemUiHider.java|AndroidProject/Boilerplate/JavaUtilSystemUiHider.txt",
                pathToJavaCode + "/util/SystemUiHiderBase.java|AndroidProject/Boilerplate/JavaUtilSystemUiHiderBase.txt",
                pathToJavaCode + "/util/SystemUiHiderHoneycomb.java|AndroidProject/Boilerplate/JavaUtilSystemUiHiderHoneycomb.txt",
                "app/src/main/AndroidManifest.xml|AndroidProject/Boilerplate/AndroidManifestXml.txt",
                "app/src/main/ic_launcher-web.png|AndroidProject/Boilerplate/IcLauncherWeb.png",
                ".idea/copyright/profiles_settings.xml|AndroidProject/Boilerplate/Idea/CopyrightProfilesSettingsXml.txt",
                ".idea/libraries/support_v4_18_0_0.xml|AndroidProject/Boilerplate/Idea/LibrariesSupportXml.txt",
                ".idea/scopes/scope_settings.xml|AndroidProject/Boilerplate/Idea/ScopeSettingsXml.txt",
                ".idea/.name|AndroidProject/Boilerplate/Idea/DotName.txt",
                ".idea/compiler.xml|AndroidProject/Boilerplate/Idea/CompilerXml.txt",
                ".idea/encodings.xml|AndroidProject/Boilerplate/Idea/EncodingsXml.txt",
                ".idea/gradle.xml|AndroidProject/Boilerplate/Idea/GradleXml.txt",
                ".idea/misc.xml|AndroidProject/Boilerplate/Idea/MiscXml.txt",
                ".idea/modules.xml|AndroidProject/Boilerplate/Idea/ModulesXml.txt",
                ".idea/vcs.xml|AndroidProject/Boilerplate/Idea/VcsXml.txt",
                ".idea/workspace.xml|AndroidProject/Boilerplate/Idea/WorkspaceXml.txt",

                // Crayon Android stuff
                pathToJavaCode + "/AndroidTranslationHelper.java|AndroidProject/AndroidTranslationHelper.txt",
                pathToJavaCode + "/GlUtil.java|AndroidProject/GlUtil.txt",
                pathToJavaCode + "/GameActivity.java|AndroidProject/GameActivity.txt",
                pathToJavaCode + "/OpenGlRenderer.java|AndroidProject/OpenGlRenderer.txt",
                pathToJavaCode + "/CrayonGlRenderer.java|AndroidProject/CrayonGlRenderer.txt",
                pathToJavaCode + "/CrayonGlSurfaceView.java|AndroidProject/CrayonGlSurfaceView.txt",

                // Generic Crayon Java stuff
                pathToJavaCode + "/AsyncMessageQueue.java|Project/AsyncMessageQueue.txt",
                pathToJavaCode + "/TranslationHelper.java|Project/TranslationHelper.txt",
                pathToJavaCode + "/Image.java|Project/Image.txt",
            })
            {
                string[] parts = simpleCopyText.Split('|');
                string target = parts[0];
                string source = parts[1];
                bool isBinary = !source.EndsWith(".txt");
                if (isBinary)
                {
                    output[target] = new FileOutput()
                    {
                        Type = FileOutputType.Binary,
                        BinaryContent = Util.ReadResourceBytesInternally("java-common/" + source)
                    };
                }
                else
                {
                    string text = Util.ReadResourceFileInternally("java-common/" + source);
                    output[target] = new FileOutput()
                    {
                        Type = FileOutputType.Text,
                        TextContent = Constants.DoReplacements(text, replacements)
                    };
                }
            }

            foreach (string basicFile in new string[] {
                "AsyncMessageQueue",
                "TranslationHelper",
                "Image",
            })
            {
                output[pathToJavaCode + "/" + basicFile + ".java"] = new FileOutput()
                {
                    Type = FileOutputType.Text,
                    TextContent = Constants.DoReplacements(
                        Util.ReadResourceFileInternally("java-common/" + basicFile + ".txt"),
                        replacements)
                };
            }

            string[] items = finalCode.Keys.OrderBy<string, string>(s => s.ToLowerInvariant()).ToArray();

            List<string> crayonWrapper = new List<string>();
            crayonWrapper.Add(string.Join("\n",
                "package %%%PACKAGE%%%;",
                "",
                "import java.util.ArrayList;",
                "import java.util.HashMap;",
                "import java.util.Stack;",
                "",
                "final class CrayonWrapper {",
                "    private CrayonWrapper() {}",
                ""));

            this.Translator.CurrentIndention++;
            foreach (string finalCodeKey in items)
            {
                this.Translator.Translate(crayonWrapper, finalCode[finalCodeKey]);
            }
            this.Translator.CurrentIndention--;

            crayonWrapper.Add(string.Join("\n",
                "}",
                ""));

            output[pathToJavaCode + "/CrayonWrapper.java"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = Constants.DoReplacements(string.Join("", crayonWrapper), replacements)
            };

            string nl = this.Translator.NL;

            foreach (StructDefinition structDefinition in structDefinitions)
            {
                string structName = structDefinition.Name.Value;
                string filename = structName + ".java";

                List<string> codeContents = new List<string>();

                codeContents.Add("class " + structName + " {" + nl);
                codeContents.Add("    public " + structName + "(");
                List<string> types = new List<string>();
                for (int i = 0; i < structDefinition.FieldsByIndex.Length; ++i)
                {
                    string type;
                    Annotation typeAnnotation = structDefinition.Types[i];
                    if (typeAnnotation == null)
                    {
                        throw new Exception("Are there any of these left?");
                    }
                    else
                    {
                        type = this.GetTypeStringFromAnnotation(typeAnnotation.FirstToken, typeAnnotation.GetSingleArgAsString(null), false, false);
                    }
                    types.Add(type);

                    if (i > 0) codeContents.Add(", ");
                    codeContents.Add(type);
                    codeContents.Add(" v_" + structDefinition.FieldsByIndex[i]);
                }
                codeContents.Add(") {" + nl);

                for (int i = 0; i < structDefinition.FieldsByIndex.Length; ++i)
                {
                    codeContents.Add("        this." + structDefinition.FieldsByIndex[i] + " = v_" + structDefinition.FieldsByIndex[i] + ";" + nl);
                }

                codeContents.Add("    }" + nl + nl);
                for (int i = 0; i < structDefinition.FieldsByIndex.Length; ++i)
                {
                    codeContents.Add("    public ");
                    codeContents.Add(types[i]);
                    codeContents.Add(" " + structDefinition.FieldsByIndex[i] + ";" + nl);
                }

                codeContents.Add("}" + nl);

                string fileContents = string.Join("", codeContents);
                string header = "package " + package + ";" + nl + nl;

                bool useList = fileContents.Contains("ArrayList<");
                bool useHashMap = fileContents.Contains("HashMap<");
                bool useStack = fileContents.Contains("Stack<");
                if (useList || useHashMap || useStack)
                {
                    if (useList) header += "import java.util.ArrayList;" + nl;
                    if (useHashMap) header += "import java.util.HashMap;" + nl;
                    if (useStack) header += "import java.util.Stack;" + nl;
                    header += nl;
                }
                fileContents = header + fileContents;

                output[pathToJavaCode + "/" + filename] = new FileOutput()
                {
                    Type = FileOutputType.Text,
                    TextContent = fileContents
                };

                output["app/src/main/res/bytecode/ByteCode.txt"] = resourceDatabase.ByteCodeFile;
            }
            /*
            foreach (string file in filesToCopyOver)
            {
                SystemBitmap bmpHack = null;
                if (file.ToLowerInvariant().EndsWith(".png"))
                {
                    bmpHack = new SystemBitmap(FileUtil.JoinPath(inputFolder, file));
                }

                if (bmpHack != null)
                {
                    output["app/src/main/res/images/" + file] = new FileOutput()
                    {
                        Type = FileOutputType.Image,
                        Bitmap = bmpHack
                    };
                }
                else
                {
                    output["app/src/main/res/images/" + file] = new FileOutput()
                    {
                        Type = FileOutputType.Copy,
                        RelativeInputPath = file
                    };
                }
            }
            ///*/

            output["app/src/main/res/raw/bytes.txt"] = resourceDatabase.ByteCodeFile;

            return output;
        }
    }
}
