using System.Collections.Generic;
using Common;

namespace Crayon.Translator.Ruby
{
    internal class RubyPlatform : AbstractPlatform
    {
        public RubyPlatform()
            : base(PlatformId.RUBY_GOSU, LanguageId.RUBY, new RubyTranslator(), new RubySystemFunctionTranslator())
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

        public override string PlatformShortId { get { return "game-ruby-gosu"; } }

        public override Dictionary<string, FileOutput> Package(
            BuildContext buildContext,
            string projectId,
            Dictionary<string, ParseTree.Executable[]> finalCode,
            ICollection<ParseTree.StructDefinition> structDefinitions,
            ResourceDatabase resourceDatabase,
            SystemLibraryManager libraryManager)
        {
            Dictionary<string, FileOutput> files = new Dictionary<string, FileOutput>();
            List<string> concatenatedCode = new List<string>();

            this.Translator.TranslateGlobals(concatenatedCode, finalCode);
            concatenatedCode.Add(this.Translator.NL);
            //this.Translator.TranslateSwitchLookups(concatenatedCode, finalCode);
            concatenatedCode.Add(this.Translator.NL);
            this.Translator.TranslateFunctions(concatenatedCode, finalCode);
            concatenatedCode.Add(this.Translator.NL);

            Dictionary<string, string> replacements = new Dictionary<string, string>()
            {
                { "PROJECT_ID", projectId },
            };

            foreach (string file in new string[] {
                "header.rb",
                "crayonHelper.rb",
                "resourceHelper.rb",
                "gfxRenderer.rb",
                "gameWindow.rb",
                "library.rb",
                "footer.rb",
            })
            {
                string content;
                if (file == "library.rb")
                {
                    content = libraryManager.EmbeddedContent;
                }
                else
                {
                    content = this.GetRubyGameCode(file, replacements);
                }
                concatenatedCode.Add(content);
                concatenatedCode.Add(this.Translator.NL);
            }


            files["run.rb"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = string.Join("", concatenatedCode),
            };

            files["resources/byte_code.txt"] = resourceDatabase.ByteCodeFile;
            if (resourceDatabase.ImageSheetManifestFile != null)
            {
                files["resources/image_sheet_manifest.txt"] = resourceDatabase.ImageSheetManifestFile;
            }
            files["resources/manifest.txt"] = resourceDatabase.ResourceManifestFile;

            foreach (FileOutput textFile in resourceDatabase.TextResources)
            {
                files["resources/text/" + textFile.CanonicalFileName] = textFile;
            }

            foreach (FileOutput audioFile in resourceDatabase.AudioResources)
            {
                files["resources/audio/" + audioFile.CanonicalFileName] = audioFile;
            }

            Dictionary<string, FileOutput> imageSheetTiles = resourceDatabase.ImageSheetFiles;
            if (imageSheetTiles != null)
            {
                foreach (string tileName in imageSheetTiles.Keys)
                {
                    files["resources/images/" + tileName] = imageSheetTiles[tileName];
                }
            }

            foreach (FileOutput image in resourceDatabase.ImageResources)
            {
                if (image.Type != FileOutputType.Ghost)
                {
                    files["resources/images/" + image.CanonicalFileName] = image;
                }
            }

            return files;
        }

        private string GetRubyGameCode(string file, Dictionary<string, string> replacements)
        {
            string rubyGosuCode = Util.ReadResourceFileInternally("game-ruby-gosu/" + file);
            rubyGosuCode = Constants.DoReplacements(false, rubyGosuCode, replacements);
            return rubyGosuCode;
        }
    }
}
