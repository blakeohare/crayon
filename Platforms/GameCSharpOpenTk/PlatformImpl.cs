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
        public override string Name { get { return "game-csharp-opentk-cbx"; } }
        public override string InheritsFrom { get { return "lang-csharp"; } }
        public override string NL { get { return "\r\n"; } }

        public PlatformImpl()
        {
            this.Translator = new CSharpOpenTkTranslator(this);
        }

        public override Dictionary<string, FileOutput> Export(
            Dictionary<string, object[]> executablesPerCompilationUnit,
            object[] structDefinitions)
        {
            throw new NotImplementedException();
        }

        public override IDictionary<string, object> GetConstantFlags()
        {
            return new Dictionary<string, object>();
        }

        public override Dictionary<string, string> GenerateReplacementDictionary(Options options)
        {
            return Util.FlattenDictionary(
                this.ParentPlatform.GenerateReplacementDictionary(options),
                new Dictionary<string, string>() {
                    { "PROJECT_GUID", CSharpHelper.GenerateGuid(options.GetStringOrNull(ExportOptionKey.GUID_SEED), "project") },
                    { "ASSEMBLY_GUID", CSharpHelper.GenerateGuid(options.GetStringOrNull(ExportOptionKey.GUID_SEED), "assembly") },
                    { "EMBEDDED_RESOURCES", string.Join("\r\n", new string[] {/* TODO: embedded resources */ }) },
                    { "CSHARP_APP_ICON", options.GetBool(ExportOptionKey.HAS_ICON) ? "<ApplicationIcon>icon.ico</ApplicationIcon>" : "" },
                    { "CSHARP_CONTENT_ICON", options.GetBool(ExportOptionKey.HAS_ICON) ? "<EmbeddedResource Include=\"icon.ico\" />" : "" }
                });
        }

        public override Dictionary<string, FileOutput> ExportProject(
            IList<VariableDeclaration> globals,
            IList<StructDefinition> structDefinitions,
            IList<FunctionDefinition> functionDefinitions,
            Options options)
        {
            Dictionary<string, string> replacements = this.GenerateReplacementDictionary(options);
            Dictionary<string, FileOutput> output = new Dictionary<string, FileOutput>();
            string projectId = options.GetString(ExportOptionKey.PROJECT_ID);
            string baseDir = projectId + "/";

            // From LangCSharp
            this.CopyResourceAsText(output, baseDir + "Vm/TranslationHelper.cs", "Resources/TranslationHelper.txt", replacements);

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

            return output;
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
