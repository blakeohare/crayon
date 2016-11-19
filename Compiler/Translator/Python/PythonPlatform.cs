using System.Collections.Generic;

namespace Crayon.Translator.Python
{
    class PythonPlatform : AbstractPlatform
    {
        public PythonPlatform()
            : base(PlatformId.PYTHON_PYGAME, LanguageId.PYTHON, new PythonTranslator(), new PythonSystemFunctionTranslator())
        { }

        public override bool IsAsync { get { return false; } }
        public override bool RemoveBreaksFromSwitch { get { return true; } }
        public override bool SupportsListClear { get { return false; } }
        public override bool IsStronglyTyped { get { return false; } }
        public override bool IsArraySameAsList { get { return true; } }
        public override bool IsCharANumber { get { return false; } }
        public override bool IntIsFloor { get { return false; } }
        public override bool IsThreadBlockingAllowed { get { return true; } }
		public override bool SupportsIncrement { get { return false; } }

        public override string PlatformShortId { get { return "game-python-pygame"; } }

        public override Dictionary<string, FileOutput> Package(
            BuildContext buildContext,
            string projectId,
            Dictionary<string, ParseTree.Executable[]> finalCode,
            ICollection<ParseTree.StructDefinition> structDefinitions,
            ResourceDatabase resourceDatabase,
            SystemLibraryManager libraryManager)
        {
            Dictionary<string, FileOutput> output = new Dictionary<string, FileOutput>();
            List<string> concatenatedCode = new List<string>();
            Dictionary<string, string> replacements = new Dictionary<string, string>()
            {
                { "PROJECT_ID", projectId },
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

            foreach (string file in new string[] {
                "Imports.py",
                "Header.py",
                "Library.py",
                "ImageHelper.py",
                "GfxRenderer.py",
                "ResourceReader.py",
                "GamepadLibraryHelper.py",
            })
            {
                string content;
                if (file == "Library.py")
                {
                    content = libraryManager.EmbeddedContent;
                }
                else
                {
                    content = this.GetPyGameCode(file, replacements);
                }
                concatenatedCode.Add(content);
                concatenatedCode.Add(this.Translator.NL);
            }

            this.Translator.TranslateGlobals(concatenatedCode, finalCode);
            concatenatedCode.Add(this.Translator.NL);
            this.Translator.TranslateSwitchLookups(concatenatedCode, finalCode);
            concatenatedCode.Add(this.Translator.NL);
            this.Translator.TranslateFunctions(concatenatedCode, finalCode);
            concatenatedCode.Add(this.Translator.NL);

            concatenatedCode.Add(this.GetPyGameCode("Footer.py", replacements));
            concatenatedCode.Add(this.Translator.NL);

            output["run.py"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = string.Join("", concatenatedCode)
            };

            output["resources/byte_code.txt"] = resourceDatabase.ByteCodeFile;
            if (resourceDatabase.ImageSheetManifestFile != null)
            {
                output["resources/image_sheet_manifest.txt"] = resourceDatabase.ImageSheetManifestFile;
            }
            output["resources/manifest.txt"] = resourceDatabase.ResourceManifestFile;

            foreach (FileOutput textFile in resourceDatabase.TextResources)
            {
                output["resources/text/" + textFile.CanonicalFileName] = textFile;
            }

            foreach (FileOutput audioFile in resourceDatabase.AudioResources)
            {
                output["resources/audio/" + audioFile.CanonicalFileName] = audioFile;
            }

            Dictionary<string, FileOutput> imageSheetTiles = resourceDatabase.ImageSheetFiles;
            if (imageSheetTiles != null)
            {
                foreach (string tileName in imageSheetTiles.Keys)
                {
                    output["resources/images/" + tileName] = imageSheetTiles[tileName];
                }
            }

            foreach (FileOutput image in resourceDatabase.ImageResources)
            {
                if (image.Type != FileOutputType.Ghost)
                {
                    output["resources/images/" + image.CanonicalFileName] = image;
                }
            }

            return output;
        }

        private string GetPyGameCode(string file, Dictionary<string, string> replacements)
        {
            string pygameCode = Util.ReadResourceFileInternally("game-python-pygame/" + file);
            pygameCode = Constants.DoReplacements(pygameCode, replacements);
            return pygameCode;
        }
    }
}
