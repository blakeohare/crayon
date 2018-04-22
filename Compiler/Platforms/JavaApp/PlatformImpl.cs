using Common;
using Pastel.Nodes;
using Pastel.Transpilers;
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
        {
            this.ContextFreePlatformImpl = new ContextFreeJavaAppPlatform();
            this.Translator = new JavaTranslator(false);
        }

        public override void ExportStandaloneVm(
            Dictionary<string, FileOutput> output,
            IList<VariableDeclaration> globals,
            IList<StructDefinition> structDefinitions,
            IList<FunctionDefinition> functionDefinitions,
            IList<LibraryForExport> everyLibrary,
            ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform)
        {
            throw new NotImplementedException();
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
            TranspilerContext ctx = new TranspilerContext();
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
                this.GenerateCodeForStruct(ctx, this.Translator, structDef);
                output["src/org/crayonlang/interpreter/structs/" + structDef.NameToken.Value + ".java"] = new FileOutput()
                {
                    Type = FileOutputType.Text,
                    TextContent = LangJava.PlatformImpl.WrapStructCodeWithImports(this.NL, ctx.FlushAndClearBuffer()),
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
                this.GenerateCodeForFunction(ctx, this.Translator, fnDef);
                this.Translator.TabDepth = 1;
                sb.Append(ctx.FlushAndClearBuffer());
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

            this.GenerateCodeForGlobalsDefinitions(ctx, this.Translator, globals);
            output["src/org/crayonlang/interpreter/VmGlobal.java"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = ctx.FlushAndClearBuffer(),
            };

            // common Java helper files
            this.CopyResourceAsText(output, "src/org/crayonlang/interpreter/FastList.java", "Resources/FastList.txt", replacements);
            this.CopyResourceAsText(output, "src/org/crayonlang/interpreter/LibraryFunctionPointer.java", "Resources/LibraryFunctionPointer.txt", replacements);
            this.CopyResourceAsText(output, "src/org/crayonlang/interpreter/LibraryInstance.java", "Resources/LibraryInstance.txt", replacements);
            this.CopyResourceAsText(output, "src/org/crayonlang/interpreter/TranslationHelper.java", "Resources/TranslationHelper.txt", replacements);
            this.CopyResourceAsText(output, "src/org/crayonlang/interpreter/PlatformTranslationHelper.java", "Resources/PlatformTranslationHelper.txt", replacements);

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
        }

        public override void GenerateCodeForFunction(TranspilerContext sb, AbstractTranslator translator, FunctionDefinition funcDef)
        {
            this.ParentPlatform.GenerateCodeForFunction(sb, translator, funcDef);
        }

        public override void GenerateCodeForGlobalsDefinitions(TranspilerContext sb, AbstractTranslator translator, IList<VariableDeclaration> globals)
        {
            this.ParentPlatform.GenerateCodeForGlobalsDefinitions(sb, translator, globals);
        }

        public override void GenerateCodeForStruct(TranspilerContext sb, AbstractTranslator translator, StructDefinition structDef)
        {
            this.ParentPlatform.GenerateCodeForStruct(sb, translator, structDef);
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
