using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Pastel.Nodes;
using Platform;

namespace GameJavaAwt
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

            TODO.OverrideJavaPackagesFromBuildFile();

            string package = options.GetString(ExportOptionKey.PROJECT_ID).ToLower();
            string sourcePath = "src/" + package + "/";

            foreach (LibraryForExport library in libraries)
            {
                if (library.ManifestFunction != null)
                {
                    this.Translator.CurrentLibraryFunctionTranslator = libraryNativeInvocationTranslatorProviderForPlatform.GetTranslator(library.Name);

                    List<string> libraryCode = new List<string>()
                    {
                        "package org.crayonlang.libraries." + library.Name.ToLower() + ";",
                        "",
                        "import java.util.ArrayList;",
                        "import java.util.HashMap;",
                        "import org.crayonlang.interpreter.Interpreter;",
                        "import org.crayonlang.interpreter.ResourceReader;",
                        "import org.crayonlang.interpreter.TranslationHelper;",
                        "import org.crayonlang.interpreter.AwtTranslationHelper;",
                        "import org.crayonlang.interpreter.VmGlobal;",
                        "import org.crayonlang.interpreter.structs.*;",
                        "",
                        "public final class LibraryWrapper {",
                        "  private LibraryWrapper() {}",
                        "",
                    };
                    this.Translator.TabDepth = 1;
                    libraryCode.Add(this.GenerateCodeForFunction(this.Translator, library.ManifestFunction));
                    foreach (FunctionDefinition fnDef in library.Functions)
                    {
                        libraryCode.Add(this.GenerateCodeForFunction(this.Translator, fnDef));
                    }
                    this.Translator.TabDepth = 0;
                    libraryCode.Add("}");
                    libraryCode.Add("");

                    output["src/org/crayonlang/libraries/" + library.Name.ToLower() + "/LibraryWrapper.java"] = new FileOutput()
                    {
                        Type = FileOutputType.Text,
                        TextContent = string.Join(this.NL, libraryCode),
                    };

                    foreach (ExportEntity supFile in library.ExportEntities["COPY_CODE"])
                    {
                        output[supFile.Values["target"]] = supFile.FileOutput;
                    }
                }
            }

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

            this.CopyResourceAsText(output, "src/org/crayonlang/interpreter/TranslationHelper.java", "Resources/TranslationHelper.txt", replacements);
            this.CopyResourceAsText(output, "src/org/crayonlang/interpreter/LibraryLoader.java", "Resources/LibraryLoader.txt", replacements);
            this.CopyResourceAsText(output, "src/org/crayonlang/interpreter/LibraryInstance.java", "Resources/LibraryInstance.txt", replacements);
            this.CopyResourceAsText(output, "src/org/crayonlang/interpreter/ResourceReader.java", "Resources/ResourceReader.txt", replacements);
            this.CopyResourceAsText(output, "src/org/crayonlang/interpreter/AwtTranslationHelper.java", "Resources/AwtTranslationHelper.txt", replacements);

            // need to move these to library supplemental files
            this.CopyResourceAsText(output, "src/org/crayonlang/interpreter/GameWindow.java", "Resources/GameWindow.txt", replacements);
            this.CopyResourceAsText(output, "src/org/crayonlang/interpreter/RenderEngine.java", "Resources/RenderEngine.txt", replacements);

            this.CopyResourceAsText(output, "src/" + package + "/Main.java", "Resources/Main.txt", replacements);
            this.CopyResourceAsText(output, "build.xml", "Resources/BuildXml.txt", replacements);

            output["resources/manifest.txt"] = resourceDatabase.ResourceManifestFile;
            output["resources/bytecode.txt"] = resourceDatabase.ByteCodeFile;
            output["resources/imagesheetmanifest.txt"] = resourceDatabase.ImageSheetManifestFile;

            foreach (string imageSheetFileName in resourceDatabase.ImageSheetFiles.Keys)
            {
                FileOutput imageSheetFile = resourceDatabase.ImageSheetFiles[imageSheetFileName];
                output["resources/images/" + imageSheetFileName] = imageSheetFile;
            }

            foreach (FileOutput textResource in resourceDatabase.TextResources)
            {
                output["resources/text/" + textResource.CanonicalFileName] = textResource;
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
            Dictionary<string, string> replacements = this.ParentPlatform.GenerateReplacementDictionary(options, resDb);
            replacements["PROJECT_ID"] = options.GetString(ExportOptionKey.PROJECT_ID);
            replacements["PACKAGE"] = options.GetString(ExportOptionKey.PROJECT_ID).ToLower();
            return replacements;
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
