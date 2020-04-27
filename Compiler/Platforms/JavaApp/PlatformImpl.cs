using Common;
using Platform;
using System;
using System.Collections.Generic;
using System.Linq;

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
            IList<LibraryForExport> everyLibrary)
        {
            throw new NotImplementedException();
        }

        public override void ExportProject(
            Dictionary<string, FileOutput> output,
            string byteCode,
            IList<LibraryForExport> libraries,
            Build.ResourceDatabase resourceDatabase,
            Options options)
        {
            Dictionary<string, string> replacements = this.GenerateReplacementDictionary(options, resourceDatabase);

            string srcPath = "src";
            string srcPackagePath = srcPath + "/" + replacements["JAVA_PACKAGE"].Replace('.', '/') + "/";

            List<string> imports = new List<string>()
            {
                "import java.util.ArrayList;",
                "import java.util.HashMap;",
                "import org.crayonlang.interpreter.Interpreter;",
                "import org.crayonlang.interpreter.TranslationHelper;",
                "import org.crayonlang.interpreter.ResourceReader;",
                "import org.crayonlang.interpreter.structs.*;",
            };

            imports.Sort();
            TemplateReader templateReader = new TemplateReader(new PkgAwareFileUtil(), this);

            foreach (LibraryForExport library in libraries.Where(t => t.HasNativeCode))
            {
                TemplateSet libraryTemplates = templateReader.GetLibraryTemplates(library);

                string libraryPath = srcPath + "/org/crayonlang/libraries/" + library.Name.ToLowerInvariant();

                foreach (string templatePath in libraryTemplates.GetPaths("gen/").Concat(libraryTemplates.GetPaths("source/")))
                {
                    string code = libraryTemplates.GetText(templatePath);
                    string realPath = templatePath.Substring(templatePath.IndexOf('/') + 1);
                    output[libraryPath + "/" + realPath] = new FileOutput()
                    {
                        Type = FileOutputType.Text,
                        TextContent = code,
                        TrimBomIfPresent = realPath.EndsWith(".java"),
                    };
                }
            }

            TemplateSet vmTemplates = templateReader.GetVmTemplates();

            output["src/org/crayonlang/interpreter/vm/CrayonWrapper.java"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = vmTemplates.GetText("CrayonWrapper.java"),
                TrimBomIfPresent = true,
            };

            foreach (string structKey in vmTemplates.GetPaths("structs/"))
            {
                string structName = System.IO.Path.GetFileNameWithoutExtension(structKey);
                string structCode = vmTemplates.GetText(structKey);

                output["src/org/crayonlang/interpreter/structs/" + structName + ".java"] = new FileOutput()
                {
                    Type = FileOutputType.Text,
                    TextContent = structCode,
                    TrimBomIfPresent = true,
                };
            }

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
            output["resources/bytecode.txt"] = new FileOutput() { Type = FileOutputType.Text, TextContent = byteCode };
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
                .Where(filename => filename.ToLowerInvariant().EndsWith(".java"))
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

        public override Dictionary<string, string> GenerateReplacementDictionary(Options options, Build.ResourceDatabase resDb)
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
