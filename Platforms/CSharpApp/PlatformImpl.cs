using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Platform;
using Pastel.Nodes;
using System.Text;

namespace GameCSharpOpenTk
{
    public class PlatformImpl : AbstractPlatform
    {
        public override string Name { get { return "csharp-app"; } }
        public override string InheritsFrom { get { return "lang-csharp"; } }
        public override string NL { get { return "\r\n"; } }

        public PlatformImpl()
        {
            this.Translator = new CSharpAppTranslator(this);
        }
        
        public override IDictionary<string, object> GetConstantFlags()
        {
            return new Dictionary<string, object>();
        }

        public override Dictionary<string, string> GenerateReplacementDictionary(Options options, ResourceDatabase resDb)
        {
            List<string> embeddedResources = new List<string>();
            foreach (FileOutput imageFile in resDb.ImageResources.Where(img => img.CanonicalFileName != null))
            {
                embeddedResources.Add("<EmbeddedResource Include=\"Resources\\" + imageFile.CanonicalFileName + "\"/>");
            }

            foreach (string imageSheetFileName in resDb.ImageSheetFiles.Keys)
            {
                embeddedResources.Add("<EmbeddedResource Include=\"Resources\\" + imageSheetFileName + "\"/>");
            }

            foreach (FileOutput textFile in resDb.TextResources.Where(img => img.CanonicalFileName != null))
            {
                embeddedResources.Add("<EmbeddedResource Include=\"Resources\\" + textFile.CanonicalFileName + "\"/>");
            }

            foreach (FileOutput audioFile in resDb.AudioResources.Where(file => file.CanonicalFileName != null))
            {
                embeddedResources.Add("<EmbeddedResource Include=\"Resources\\" + audioFile.CanonicalFileName + "\"/>");
            }

            return Util.FlattenDictionary(
                this.ParentPlatform.GenerateReplacementDictionary(options, resDb),
                new Dictionary<string, string>() {
                    { "PROJECT_GUID", CSharpHelper.GenerateGuid(options.GetStringOrNull(ExportOptionKey.GUID_SEED), "project") },
                    { "ASSEMBLY_GUID", CSharpHelper.GenerateGuid(options.GetStringOrNull(ExportOptionKey.GUID_SEED), "assembly") },
                    { "EMBEDDED_RESOURCES", string.Join("\r\n", embeddedResources) },
                    { "CSHARP_APP_ICON", options.GetBool(ExportOptionKey.HAS_ICON) ? "<ApplicationIcon>icon.ico</ApplicationIcon>" : "" },
                    { "CSHARP_CONTENT_ICON", options.GetBool(ExportOptionKey.HAS_ICON) ? "<EmbeddedResource Include=\"icon.ico\" />" : "" }
                });
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
            string projectId = options.GetString(ExportOptionKey.PROJECT_ID);
            string baseDir = projectId + "/";

            // From LangCSharp
            this.CopyResourceAsText(output, baseDir + "Vm/TranslationHelper.cs", "Resources/TranslationHelper.txt", replacements);
            this.CopyResourceAsText(output, baseDir + "Vm/Library.cs", "Resources/Library.txt", replacements);
            this.CopyResourceAsText(output, baseDir + "Vm/LibraryFunctionPointer.cs", "Resources/LibraryFunctionPointer.txt", replacements);
            this.CopyResourceAsText(output, baseDir + "Vm/LibraryRegistry.cs", "Resources/LibraryRegistry.txt", replacements);

            // Project files from CSharpOpenTK
            this.CopyResourceAsText(output, projectId + ".sln", "Resources/SolutionFile.txt", replacements);
            this.CopyResourceAsText(output, baseDir + "Interpreter.csproj", "Resources/ProjectFile.txt", replacements);

            // Required project files
            this.CopyResourceAsText(output, baseDir + "Properties/AssemblyInfo.cs", "Resources/AssemblyInfo.txt", replacements);
            this.CopyResourceAsText(output, baseDir + "Program.cs", "Resources/Program.txt", replacements);

            // CSharpOpenTK specific stuff
            this.CopyResourceAsText(output, baseDir + "OtkGame/GamepadTranslationHelper.cs", "Resources/GamepadTranslationHelper.txt", replacements);
            this.CopyResourceAsText(output, baseDir + "OtkGame/GameWindow.cs", "Resources/GameWindow.txt", replacements);
            this.CopyResourceAsText(output, baseDir + "OtkGame/GlUtil.cs", "Resources/GlUtil.txt", replacements);
            this.CopyResourceAsText(output, baseDir + "OtkGame/OpenTkRenderer.cs", "Resources/OpenTkRenderer.txt", replacements);
            this.CopyResourceAsText(output, baseDir + "OtkGame/OpenTkTranslationHelper.cs", "Resources/OpenTkTranslationHelper.txt", replacements);
            this.CopyResourceAsText(output, baseDir + "OtkGame/ResourceReader.cs", "Resources/ResourceReader.txt", replacements);

            // Text from CSharpOpenTK
            this.CopyResourceAsText(output, baseDir + "DependencyLicenses.txt", "Resources/DependencyLicenses.txt", replacements);

            // DLL's from CSharpOpenTK
            this.CopyResourceAsBinary(output, baseDir + "OpenTK.dll", "Resources/DllOpenTk.binary");
            this.CopyResourceAsBinary(output, baseDir + "SDL.dll", "Resources/DllSdl.binary");
            this.CopyResourceAsBinary(output, baseDir + "SDL_mixer.dll", "Resources/DllSdlMixer.binary");
            this.CopyResourceAsBinary(output, baseDir + "SdlDotNet.dll", "Resources/DllSdlDotNet.binary");
            this.CopyResourceAsBinary(output, baseDir + "Tao.Sdl.dll", "Resources/DllTaoSdl.binary");
            this.CopyResourceAsBinary(output, baseDir + "libogg-0.dll", "Resources/DllLibOgg0.binary");
            this.CopyResourceAsBinary(output, baseDir + "libvorbis-0.dll", "Resources/DllLibVorbis0.binary");
            this.CopyResourceAsBinary(output, baseDir + "libvorbisfile-3.dll", "Resources/DllLibVorbisFile3.binary");

            foreach (LibraryForExport library in libraries)
            {
                this.Translator.CurrentLibraryFunctionTranslator = libraryNativeInvocationTranslatorProviderForPlatform.GetTranslator(library.Name);
                string libraryName = library.Name;
                List<string> libraryLines = new List<string>();
                if (library.ManifestFunction != null)
                {
                    libraryLines.Add(this.GenerateCodeForFunction(this.Translator, library.ManifestFunction));
                    foreach (FunctionDefinition funcDef in library.Functions)
                    {
                        libraryLines.Add(this.GenerateCodeForFunction(this.Translator, funcDef));
                    }

                    output[baseDir + "Libraries/" + libraryName + "/LibraryWrapper.cs"] = new FileOutput()
                    {
                        Type = FileOutputType.Text,
                        TextContent = string.Join(this.NL,
                            "using System;",
                            "using System.Collections.Generic;",
                            "using System.Linq;",
                            "using Interpreter.OtkGame;",
                            "using Interpreter.Structs;",
                            "using Interpreter.Vm;",
                            "",
                            "namespace Interpreter.Libraries." + libraryName,
                            "{",
                            "    public static class LibraryWrapper",
                            "    {",
                            this.TEMPORARY_HACK_replacements(libraryName, this.IndentCodeWithSpaces(string.Join(this.NL, libraryLines), 8)),
                            "    }",
                            "}",
                            ""),
                    };

                    foreach (string filename in library.SupplementalFiles.Keys)
                    {
                        output[baseDir + "Libraries/" + libraryName + "/" + filename] = library.SupplementalFiles[filename];
                    }
                }
            }

            foreach (StructDefinition structDefinition in structDefinitions)
            {
                output[baseDir + "Structs/" + structDefinition.NameToken.Value + ".cs"] = new FileOutput()
                {
                    Type = FileOutputType.Text,
                    TextContent = string.Join("\r\n", new string[] {
                        "using System;",
                        "using System.Collections.Generic;",
                        "",
                        "namespace Interpreter.Structs",
                        "{",
                        this.IndentCodeWithSpaces(this.GenerateCodeForStruct(structDefinition).Trim(), 4),
                        "}",
                        ""
                    }),
                };
            }

            List<string> coreVmFunctions = new List<string>();
            foreach (FunctionDefinition funcDef in functionDefinitions)
            {
                coreVmFunctions.Add(this.GenerateCodeForFunction(this.Translator, funcDef));
            }

            string functionCode = string.Join("\r\n\r\n", coreVmFunctions);

            output[baseDir + "Vm/CrayonWrapper.cs"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = string.Join("\r\n", new string[] {
                    "using System;",
                    "using System.Collections.Generic;",
                    "using System.Linq;",
                    "using Interpreter.Structs;",
                    "",
                    "namespace Interpreter.Vm",
                    "{",
                    "    public class CrayonWrapper",
                    "    {",
                    this.IndentCodeWithSpaces(functionCode, 8),
                    "    }",
                    "}",
                    ""
                }),
            };

            StringBuilder globalsCode = new StringBuilder();
            this.Translator.TranslateExecutables(globalsCode, globals.Cast<Executable>().ToArray());

            output[baseDir + "Vm/Globals.cs"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = string.Join("\r\n", new string[]
                {
                    "using System;",
                    "using System.Collections.Generic;",
                    "using Interpreter.Structs;",
                    "",
                    "namespace Interpreter.Vm",
                    "{",
                    this.GenerateCodeForGlobalsDefinitions(this.Translator, globals),
                    "}",
                    ""
                }),
            };

            output[baseDir + "ByteCode.txt"] = resourceDatabase.ByteCodeFile;
            output[baseDir + "ResourceManifest.txt"] = resourceDatabase.ResourceManifestFile;
            output[baseDir + "ImageSheetManifest.txt"] = resourceDatabase.ImageSheetManifestFile;

            foreach (FileOutput imageFile in resourceDatabase.ImageResources.Where(img => img.CanonicalFileName != null))
            {
                output[baseDir + "Resources/" + imageFile.CanonicalFileName] = imageFile;
            }

            foreach (string imageSheetFileName in resourceDatabase.ImageSheetFiles.Keys)
            {
                output[baseDir + "Resources/" + imageSheetFileName] = resourceDatabase.ImageSheetFiles[imageSheetFileName];
            }

            foreach (FileOutput textFile in resourceDatabase.TextResources.Where(img => img.CanonicalFileName != null))
            {
                output[baseDir + "Resources/" + textFile.CanonicalFileName] = textFile;
            }

            foreach (FileOutput audioFile in resourceDatabase.AudioResources.Where(file => file.CanonicalFileName != null))
            {
                output[baseDir + "Resources/" + audioFile.CanonicalFileName] = audioFile;
            }
            
            return output;
        }

        private string TEMPORARY_HACK_replacements(string libraryName, string content)
        {
            CompatibilityHack.CriticalTODO("Update the translations to do the right thing.");

            if (content.Contains("Library." + libraryName + "."))
            {
                return content.Replace("Library." + libraryName + ".", "");
            }
            return content;
        }

        public override string GenerateCodeForGlobalsDefinitions(AbstractTranslator translator, IList<VariableDeclaration> globals)
        {
            return this.ParentPlatform.GenerateCodeForGlobalsDefinitions(this.Translator, globals);
        }

        public override string GenerateCodeForFunction(AbstractTranslator translator, FunctionDefinition funcDef)
        {
            return this.ParentPlatform.GenerateCodeForFunction(this.Translator, funcDef);
        }

        public override string GenerateCodeForStruct(StructDefinition structDef)
        {
            return this.ParentPlatform.GenerateCodeForStruct(structDef);
        }
    }
}
