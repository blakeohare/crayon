using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Pastel.Nodes;
using Platform;

namespace JavaScriptApp
{
    public class PlatformImpl : AbstractPlatform
    {
        public override string Name { get { return "javascript-app"; } }
        public override string InheritsFrom { get { return "lang-javascript"; } }
        public override string NL { get { return "\n"; } }

        public PlatformImpl()
        {
            this.Translator = new JavaScriptAppTranslator(this);
        }

        public override IDictionary<string, object> GetConstantFlags()
        {
            return new Dictionary<string, object>();
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
            return this.ExportProjectImpl(globals, structDefinitions, functionDefinitions, libraries, resourceDatabase, options, libraryNativeInvocationTranslatorProviderForPlatform, this.Translator);
        }

        public Dictionary<string, FileOutput> ExportProjectImpl(
            IList<VariableDeclaration> globals,
            IList<StructDefinition> structDefinitions,
            IList<FunctionDefinition> functionDefinitions,
            IList<LibraryForExport> libraries,
            ResourceDatabase resourceDatabase,
            Options options,
            ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform,
            AbstractTranslator translatorOverride)
        {
            Dictionary<string, FileOutput> output = new Dictionary<string, FileOutput>();
            Dictionary<string, string> replacements = this.GenerateReplacementDictionary(options, resourceDatabase);

            List<string> coreVmCode = new List<string>();

            coreVmCode.Add(this.GenerateCodeForGlobalsDefinitions(translatorOverride, globals));

            foreach (FunctionDefinition funcDef in functionDefinitions)
            {
                coreVmCode.Add(this.GenerateCodeForFunction(translatorOverride, funcDef));
            }

            string coreVm = string.Join("\r\n", coreVmCode);

            output["vm.js"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = coreVm,
            };

            List<LibraryForExport> librariesWithCode = new List<LibraryForExport>();
            foreach (LibraryForExport library in libraries)
            {
                if (library.ManifestFunction != null)
                {
                    List<string> libraryLines = new List<string>();

                    translatorOverride.CurrentLibraryFunctionTranslator =
                        libraryNativeInvocationTranslatorProviderForPlatform.GetTranslator(library.Name);

                    library.ManifestFunction.NameToken = Pastel.Token.CreateDummyToken("lib_" + library.Name.ToLower() + "_manifest");
                    libraryLines.Add(this.GenerateCodeForFunction(translatorOverride, library.ManifestFunction));
                    foreach (FunctionDefinition fnDef in library.Functions)
                    {
                        libraryLines.Add(this.GenerateCodeForFunction(translatorOverride, fnDef));
                    }
                    libraryLines.Add("C$common$scrapeLibFuncNames('" + library.Name.ToLower() + "');");
                    libraryLines.Add("");

                    // add helper functions after the scrape.

                    foreach (ExportEntity embedCode in library.ExportEntities["EMBED_CODE"])
                    {
                        libraryLines.Add(embedCode.StringValue);
                    }

                    output["libs/lib_" + library.Name.ToLower() + ".js"] = new FileOutput()
                    {
                        Type = FileOutputType.Text,
                        TextContent = string.Join("\n", libraryLines),
                    };
                    librariesWithCode.Add(library);
                }
            }

            Dictionary<string, string> htmlReplacements = new Dictionary<string, string>(replacements);
            replacements["JS_LIB_INCLUSIONS"] = this.GenerateJsLibInclusionHtml(output.Keys);
            this.CopyResourceAsText(output, "index.html", "Resources/GameHostHtml.txt", replacements);

            this.CopyResourceAsText(output, "common.js", "Resources/Common.txt", replacements);

            TODO.JavaScriptDeGamification();
            output["lib_supplemental.js"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = string.Join("\n\n", new string[] {
                    this.LoadTextResource("Resources/ImageResource.txt", replacements),
                    this.LoadTextResource("Resources/Game.txt", replacements),
                    this.LoadTextResource("Resources/Drawing.txt", replacements),
                    this.LoadTextResource("Resources/Gamepad.txt", replacements),
                    this.LoadTextResource("Resources/Input.txt", replacements),
                    this.LoadTextResource("Resources/Sound.txt", replacements),
                }),
            };

            StringBuilder resourcesJs = new StringBuilder();

            foreach (FileOutput textResource in resourceDatabase.TextResources)
            {
                resourcesJs.Append("C$common$addTextRes(");
                resourcesJs.Append(Util.ConvertStringValueToCode(textResource.CanonicalFileName));
                resourcesJs.Append(", ");
                resourcesJs.Append(Util.ConvertStringValueToCode(textResource.TextContent));
                resourcesJs.Append(");\n");
            }

            foreach (FileOutput fontResource in resourceDatabase.FontResources)
            {
                resourcesJs.Append("C$common$addBinaryRes(");
                resourcesJs.Append(Util.ConvertStringValueToCode(fontResource.CanonicalFileName));
                resourcesJs.Append(", '");
                resourcesJs.Append(Util.ConvertByteArrayToBase64(fontResource.GetFinalBinaryContent()));
                resourcesJs.Append("');\n");
            }

            FileOutput imageSheetManifest = resourceDatabase.ImageSheetManifestFile;
            resourcesJs.Append("C$common$addTextRes('image_sheets.txt', ");
            resourcesJs.Append(imageSheetManifest == null ? "''" : Util.ConvertStringValueToCode(imageSheetManifest.TextContent));
            resourcesJs.Append(");\n");

            resourcesJs.Append("C$common$resourceManifest = ");
            resourcesJs.Append(Util.ConvertStringValueToCode(resourceDatabase.ResourceManifestFile.TextContent));
            resourcesJs.Append(";\n");

            string filePrefix = options.GetStringOrNull(ExportOptionKey.JS_FILE_PREFIX);
            if (filePrefix != null)
            {
                resourcesJs.Append("C$common$jsFilePrefix = ");
                resourcesJs.Append(Util.ConvertStringValueToCode(filePrefix));
                resourcesJs.Append(";\n");
            }

            output["resources.js"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = resourcesJs.ToString(),
            };

            output["bytecode.js"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = "C$bytecode = " + Util.ConvertStringValueToCode(resourceDatabase.ByteCodeFile.TextContent) + ";",
            };

            foreach (string imageResourceFile in resourceDatabase.ImageSheetFiles.Keys)
            {
                FileOutput file = resourceDatabase.ImageSheetFiles[imageResourceFile];
                output["resources/images/" + imageResourceFile] = file;
            }

            foreach (FileOutput audioResourceFile in resourceDatabase.AudioResources)
            {
                output["resources/audio/" + audioResourceFile.CanonicalFileName] = audioResourceFile;
            }

            // TODO: minify JavaScript across all of output dictionary

            return output;
        }

        public override string GenerateCodeForStruct(StructDefinition structDef)
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

        public string GenerateJsLibInclusionHtml(ICollection<string> filesIncluded)
        {
            string[] libraryPathsIncluded = filesIncluded.Where(s => s.StartsWith("libs/lib_")).OrderBy(s => s).ToArray();
            if (libraryPathsIncluded.Length > 0)
            {
                return
                    this.IndentCodeWithTabs(
                        "<script type=\"text/javascript\" src=\"" +
                        string.Join(
                            "\"></script>\n<script type=\"text/javascript\" src=\"",
                            libraryPathsIncluded) +
                        "\"></script>",
                    2);
            }
            return "";
        }

        public override Dictionary<string, string> GenerateReplacementDictionary(Options options, ResourceDatabase resDb)
        {
            return Util.MergeDictionaries(
                this.ParentPlatform.GenerateReplacementDictionary(options, resDb),
                new Dictionary<string, string>()
                {
                    { "DEFAULT_TITLE", options.GetString(ExportOptionKey.DEFAULT_TITLE, "Untitled") },
                    {
                        "FAVICON",
                        options.GetBool(ExportOptionKey.HAS_ICON)
                            ? "<link rel=\"shortcut icon\" href=\"" + options.GetStringOrEmpty(ExportOptionKey.JS_FILE_PREFIX) + "favicon.ico\">"
                            : ""
                    },
                    { "JS_EXTRA_HEAD", options.GetStringOrEmpty(ExportOptionKey.JS_HEAD_EXTRAS) },
                });
        }
    }
}
