using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Pastel.Nodes;
using Platform;

namespace JavaApp
{
    public class PlatformImpl : AbstractPlatform
    {
        public override string Name { get { return "java-app"; } }
        public override string InheritsFrom { get { return "lang-java"; } }
        public override string NL { get { return "\n"; } }

        public PlatformImpl()
        {
            this.Translator = new JavaAppTranslator(this);
        }

        public override Dictionary<string, FileOutput> ExportStandaloneVm(
            IList<VariableDeclaration> globals,
            IList<StructDefinition> structDefinitions,
            IList<FunctionDefinition> functionDefinitions,
            IList<LibraryForExport> everyLibrary,
            ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<string, FileOutput> ExportProject(
            IList<VariableDeclaration> globals,
            IList<StructDefinition> structDefinitions,
            IList<FunctionDefinition> functionDefinitions,
            IList<LibraryForExport> libraries,
            ResourceDatabase resourceDatabase,
            Options options,
            ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform)
        {
            Dictionary<string, string> replacements = this.GenerateReplacementDictionary(options, resourceDatabase);
            Dictionary<string, FileOutput> output = new Dictionary<string, FileOutput>();

            string srcPath = "src";
            string srcPackagePath = srcPath + "/" + replacements["JAVA_PACKAGE"].Replace('.', '/') + "/";

            string[] imports = new string[]
            {
                "import org.crayonlang.interpreter.ResourceReader;",
                "import org.crayonlang.interpreter.AwtTranslationHelper;",
            };

            LangJava.PlatformImpl.ExportJavaLibraries(this, srcPath, libraries, output, libraryNativeInvocationTranslatorProviderForPlatform, imports);

            foreach (StructDefinition structDef in structDefinitions)
            {
                output["src/org/crayonlang/interpreter/structs/" + structDef.NameToken.Value + ".java"] = new FileOutput()
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

            output["src/org/crayonlang/interpreter/Interpreter.java"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = sb.ToString(),
            };

            output["src/org/crayonlang/interpreter/VmGlobal.java"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = this.GenerateCodeForGlobalsDefinitions(this.Translator, globals),
            };

            // common Java helper files
            this.CopyResourceAsText(output, "src/org/crayonlang/interpreter/TranslationHelper.java", "Resources/TranslationHelper.txt", replacements);
            this.CopyResourceAsText(output, "src/org/crayonlang/interpreter/LibraryInstance.java", "Resources/LibraryInstance.txt", replacements);

            // java-app specific files
            this.CopyResourceAsText(output, "src/org/crayonlang/interpreter/LibraryLoader.java", "Resources/LibraryLoader.txt", replacements);
            this.CopyResourceAsText(output, "src/org/crayonlang/interpreter/ResourceReader.java", "Resources/ResourceReader.txt", replacements);
            this.CopyResourceAsText(output, "src/org/crayonlang/interpreter/AwtTranslationHelper.java", "Resources/AwtTranslationHelper.txt", replacements);

            // need to move these to library supplemental files
            this.CopyResourceAsText(output, "src/org/crayonlang/interpreter/GameWindow.java", "Resources/GameWindow.txt", replacements);
            this.CopyResourceAsText(output, "src/org/crayonlang/interpreter/RenderEngine.java", "Resources/RenderEngine.txt", replacements);

            this.CopyResourceAsText(output, srcPackagePath + "/Main.java", "Resources/Main.txt", replacements);
            this.CopyResourceAsText(output, "build.xml", "Resources/BuildXml.txt", replacements);

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

            return output;
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
