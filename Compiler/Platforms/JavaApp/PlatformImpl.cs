using Common;
using Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JavaApp
{
    public class PlatformImpl : AbstractPlatform
    {
        public override string Name { get { return "java-app"; } }
        public override string InheritsFrom { get { return "lang-java"; } }
        public override string NL { get { return "\n"; } }

        public PlatformImpl()
            : base("JAVA")
        { }

        public override void ExportStandaloneVm(
            Dictionary<string, FileOutput> output,
            TemplateStorage templtes,
            IList<LibraryForExport> everyLibrary)
        {
            throw new NotImplementedException();
        }

        public override void ExportProject(
            Dictionary<string, FileOutput> output,
            TemplateStorage templates,
            IList<LibraryForExport> libraries,
            ResourceDatabase resourceDatabase,
            Options options)
        {
            Dictionary<string, string> replacements = this.GenerateReplacementDictionary(options, resourceDatabase);

            string srcPath = "src";
            string srcPackagePath = srcPath + "/" + replacements["JAVA_PACKAGE"].Replace('.', '/') + "/";

            string[] imports = new string[]
            {
                "import org.crayonlang.interpreter.ResourceReader;",
            };

            List<string> defaultImports = new List<string>()
            {
                "import java.util.ArrayList;",
                "import java.util.HashMap;",
                "import org.crayonlang.interpreter.Interpreter;",
                "import org.crayonlang.interpreter.TranslationHelper;",
                "import org.crayonlang.interpreter.structs.*;",
            };

            defaultImports.AddRange(imports);
            defaultImports.Sort();

            foreach (LibraryForExport library in libraries)
            {
                if (library.HasNativeCode)
                {
                    List<string> libraryCode = new List<string>()
                    {
                        "package org.crayonlang.libraries." + library.Name.ToLower() + ";",
                        "",
                    };
                    libraryCode.AddRange(defaultImports);
                    libraryCode.AddRange(new string[]
                    {
                        "",
                        "public final class LibraryWrapper {",
                        "  private LibraryWrapper() {}",
                        "",
                    });

                    libraryCode.Add(templates.GetCode("library:" + library.Name + ":functions"));

                    libraryCode.Add("}");
                    libraryCode.Add("");

                    string libraryPath = srcPath + "/org/crayonlang/libraries/" + library.Name.ToLower();

                    output[libraryPath + "/LibraryWrapper.java"] = new FileOutput()
                    {
                        Type = FileOutputType.Text,
                        TextContent = string.Join(this.NL, libraryCode),
                    };

                    foreach (string structKey in templates.GetTemplateKeysWithPrefix("library:" + library.Name + ":struct:"))
                    {
                        string structName = templates.GetName(structKey);
                        string structCode = templates.GetCode(structKey);

                        structCode = this.WrapStructCodeWithImports(this.NL, structCode);

                        // This is kind of a hack.
                        // TODO: better.
                        structCode = structCode.Replace(
                            "package org.crayonlang.interpreter.structs;",
                            "package org.crayonlang.libraries." + library.Name.ToLower() + ";");

                        output[libraryPath + "/" + structName + ".java"] = new FileOutput()
                        {
                            Type = FileOutputType.Text,
                            TextContent = structCode,
                        };
                    }

                    foreach (ExportEntity supFile in library.ExportEntities["COPY_CODE"])
                    {
                        string path = supFile.Values["target"].Replace("%LIBRARY_PATH%", libraryPath);
                        output[path] = supFile.FileOutput;
                    }
                }
            }
            foreach (string structKey in templates.GetTemplateKeysWithPrefix("vm:struct:"))
            {
                string structName = templates.GetName(structKey);
                string structCode = templates.GetCode(structKey);

                output["src/org/crayonlang/interpreter/structs/" + structName + ".java"] = new FileOutput()
                {
                    Type = FileOutputType.Text,
                    TextContent = this.WrapStructCodeWithImports(this.NL, structCode),
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

            sb.Append(templates.GetCode("vm:functions"));
            sb.Append("}");
            sb.Append(this.NL);

            output["src/org/crayonlang/interpreter/Interpreter.java"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = sb.ToString(),
            };

            // common Java helper files
            this.CopyResourceAsText(output, "src/org/crayonlang/interpreter/LibraryInstance.java", "Resources/LibraryInstance.java", replacements);
            this.CopyResourceAsText(output, "src/org/crayonlang/interpreter/TranslationHelper.java", "Resources/TranslationHelper.java", replacements);
            this.CopyResourceAsText(output, "src/org/crayonlang/interpreter/PlatformTranslationHelper.java", "Resources/PlatformTranslationHelper.java", replacements);

            // java-app specific files
            this.CopyResourceAsText(output, "src/org/crayonlang/interpreter/LibraryLoader.java", "Resources/LibraryLoader.java", replacements);
            this.CopyResourceAsText(output, "src/org/crayonlang/interpreter/ResourceReader.java", "Resources/ResourceReader.java", replacements);

            this.CopyResourceAsText(output, srcPackagePath + "/Main.java", "Resources/Main.java", replacements);
            this.CopyResourceAsText(output, "build.xml", "Resources/Build.xml", replacements);

            output["resources/manifest.txt"] = resourceDatabase.ResourceManifestFile;
            output["resources/bytecode.txt"] = resourceDatabase.ByteCodeFile;
            if (resourceDatabase.ImageSheetManifestFile != null)
            {
                output["resources/imagesheetmanifest.txt"] = resourceDatabase.ImageSheetManifestFile;
            }

            foreach (string imageSheetFileName in resourceDatabase.ImageSheetFiles.Keys)
            {
                FileOutput imageSheetFile = resourceDatabase.ImageSheetFiles[imageSheetFileName];
                output["resources/images/" + imageSheetFileName] = imageSheetFile;
            }

            foreach (FileOutput audioResource in resourceDatabase.AudioResources)
            {
                output["resources/audio/" + audioResource.CanonicalFileName] = audioResource;
            }

            foreach (FileOutput textResource in resourceDatabase.TextResources)
            {
                output["resources/text/" + textResource.CanonicalFileName] = textResource;
            }

            foreach (FileOutput fontResource in resourceDatabase.FontResources)
            {
                output["resources/ttf/" + fontResource.CanonicalFileName] = fontResource;
            }

            IEnumerable<FileOutput> javaFiles = output.Keys
                .Where(filename => filename.ToLower().EndsWith(".java"))
                .Select(filename => output[filename]);
            foreach (FileOutput file in javaFiles)
            {
                file.TrimBomIfPresent = true;
            }
        }

        private string WrapStructCodeWithImports(string nl, string original)
        {
            List<string> lines = new List<string>();
            lines.Add("package org.crayonlang.interpreter.structs;");
            lines.Add("");
            bool hasLists = original.Contains("public ArrayList<");
            bool hasDictionaries = original.Contains("public HashMap<");
            if (hasLists) lines.Add("import java.util.ArrayList;");
            if (hasDictionaries) lines.Add("import java.util.HashMap;");
            if (hasLists || hasDictionaries) lines.Add("");

            lines.Add(original);
            lines.Add("");

            return string.Join(nl, lines);
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
