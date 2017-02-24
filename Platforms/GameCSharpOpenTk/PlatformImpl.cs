using System;
using System.Collections.Generic;
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
            this.CopyResourceAsText(output, baseDir + "TranslationHelper.cs", "Resources/TranslationHelper.txt", replacements);
            this.CopyResourceAsText(output, baseDir + "ResourceReader.cs", "Resources/ResourceReader.txt", replacements);

            // Project files from CSharpOpenTK
            this.CopyResourceAsText(output, projectId + ".sln", "Resources/SolutionFile.txt", replacements);
            this.CopyResourceAsText(output, baseDir + "Interpreter.csproj", "Resources/ProjectFile.txt", replacements);

            // TODO: a lot of this needs to go into supplemental files for the related library.
            // Code from CSharpOpenTK
            this.CopyResourceAsText(output, baseDir + "Properties/AssemblyInfo.cs", "Resources/AssemblyInfo.txt", replacements);
            this.CopyResourceAsText(output, baseDir + "GamepadTranslationHelper.cs", "Resources/GamepadTranslationHelper.txt", replacements);
            this.CopyResourceAsText(output, baseDir + "GameWindow.cs", "Resources/GameWindow.txt", replacements);
            this.CopyResourceAsText(output, baseDir + "GlUtil.cs", "Resources/GlUtil.txt", replacements);
            this.CopyResourceAsText(output, baseDir + "OpenTkRenderer.cs", "Resources/OpenTkRenderer.txt", replacements);
            this.CopyResourceAsText(output, baseDir + "OpenTkTranslationHelper.cs", "Resources/OpenTkTranslationHelper.txt", replacements);
            this.CopyResourceAsText(output, baseDir + "Program.cs", "Resources/Program.txt", replacements);

            // Text from CSharpOpenTK
            this.CopyResourceAsText(output, baseDir + "License.txt", "Resources/License.txt", replacements);

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
                        this.IndentCodeWithTabs(this.GenerateCodeForStruct(structDefinition).Trim(), 1),
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
                    "using Interpreter.Structs;",
                    "",
                    "namespace Interpreter.Vm",
                    "{",
                    "\tpublic class CrayonWrapper",
                    "\t{",
                    this.IndentCodeWithTabs(functionCode, 2),
                    "\t}",
                    "}",
                    ""
                }),
            };

            return output;
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
