using System;
using System.Collections.Generic;
using System.Linq;
using Crayon.ParseTree;

namespace Crayon.Translator.Java
{
    internal class JavaAwtPlatform : JavaPlatform
    {
        public override bool TrimBom { get { return true; } } // Seriously, Oracle?

        public JavaAwtPlatform()
            : base(PlatformId.JAVA_AWT, new JavaAwtSystemFunctionTranslator())
        { }

        public override string PlatformShortId { get { return "game-java-awt"; } }

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
            string package = projectId.ToLowerInvariant();

            Dictionary<string, string> replacements = new Dictionary<string, string>() {
                { "PROJECT_TITLE", projectId },
                { "PROJECT_ID", projectId },
                { "PACKAGE", package },
                { "CURRENT_YEAR", DateTime.Now.Year.ToString() },
                { "COPYRIGHT", "©" },
            };

            bool hasIcon = buildContext.IconFilePath != null;
            if (hasIcon)
            {
                output["resources/icon.png"] = new FileOutput()
                {
                    Type = FileOutputType.Copy,
                    AbsoluteInputPath = buildContext.IconFilePath,
                };
            }

            foreach (string awtFile in new string[] {
                "AwtTranslationHelper",
            })
            {
                output["src/" + package + "/" + awtFile + ".java"] = new FileOutput()
                {
                    Type = FileOutputType.Text,
                    TextContent = Constants.DoReplacements(
                        Util.ReadResourceFileInternally("game-java-awt/" + awtFile + ".txt"),
                        replacements)
                };
            }

            foreach (string basicFile in new string[] {
                "TranslationHelper",
            })
            {
                output["src/" + package + "/" + basicFile + ".java"] = new FileOutput()
                {
                    Type = FileOutputType.Text,
                    TextContent = Constants.DoReplacements(
                        Util.ReadResourceFileInternally("java-common/" + basicFile + ".txt"),
                        replacements)
                };
            }

            foreach (string basicFile in new string[] {
                "AwtTranslationHelper",
                "Start",
                "GameWindow",
                "Image",
                "RenderEngine",
            })
            {
                output["src/" + package + "/" + basicFile + ".java"] = new FileOutput()
                {
                    Type = FileOutputType.Text,
                    TextContent = Constants.DoReplacements(
                        Util.ReadResourceFileInternally("game-java-awt/" + basicFile + ".txt"),
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
                "public final class CrayonWrapper {",
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

            output["src/" + package + "/CrayonWrapper.java"] = new FileOutput()
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

                codeContents.Add("public final class " + structName + " {" + nl);
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

                output["src/" + package + "/" + filename] = new FileOutput()
                {
                    Type = FileOutputType.Text,
                    TextContent = fileContents
                };
            }
            
            foreach (string tileFile in resourceDatabase.ImageSheetFiles.Keys)
            {
                output["resources/images/" + tileFile] = resourceDatabase.ImageSheetFiles[tileFile];
            }

            foreach (FileOutput textFile in resourceDatabase.TextResources)
            {
                output["resources/text/" + textFile.CanonicalFileName] = textFile;
            }

            Dictionary<string, string> libraryResourceFiles = this.LibraryManager.CopiedFiles;
            foreach (string file in libraryResourceFiles.Keys)
            {
                string content = libraryResourceFiles[file];
                output[file.Replace("%PROJECT_ID%", projectId.ToLower())] = new FileOutput()
                {
                    Type = FileOutputType.Text,
                    TextContent = Constants.DoReplacements(content, replacements)
                };
            }
            
            output["resources/bytecode.txt"] = resourceDatabase.ByteCodeFile;
            output["resources/manifest.txt"] = resourceDatabase.ResourceManifestFile;
            if (resourceDatabase.ImageSheetManifestFile != null)
            {
                output["resources/imagesheetmanifest.txt"] = resourceDatabase.ImageSheetManifestFile;
            }

            output["build.xml"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = Constants.DoReplacements(
                    Util.ReadResourceFileInternally("game-java-awt/BuildXml.txt"),
                    replacements)
            };

            return output;
        }
    }
}
