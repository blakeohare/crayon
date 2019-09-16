using Common;
using CommonUtil.Images;
using CommonUtil.Resources;
using System;
using System.Collections.Generic;

namespace Exporter
{
    public class DefaultProjectGenerator
    {
        private string originalProjectId;
        private string projectId;
        private Localization.Locale projectLocale;

        public string ProjectID { get { return this.projectId; } }

        public DefaultProjectGenerator(string projectId, string localeId)
        {
            this.originalProjectId = projectId.Trim();
            this.projectLocale = Localization.Locale.Get(localeId);
            this.projectId = CommonUtil.Core.StringUtil.FilterStringToAlphanumerics(this.originalProjectId);
        }

        public DefaultProjectGenerator Validate()
        {
            if (this.projectId.Length == 0) throw new InvalidOperationException("Project name did not have any alphanumeric characters.");
            if (this.projectId[0] >= '0' && this.projectId[0] <= '9') throw new InvalidOperationException("Project name cannot begin with a number.");
            if (this.projectId != this.originalProjectId)
            {
                ConsoleWriter.Print(
                    ConsoleMessageType.BUILD_WARNING,
                    "Warning: '" + this.originalProjectId + "' contains non-alphanumeric characters and was canonicalized into '" + this.projectId + "'");
            }

            return this;
        }

        private Dictionary<string, string> replacements;

        public Dictionary<string, FileOutput> Export()
        {
            this.replacements = new Dictionary<string, string>()
            {
                { "PROJECT_ID", this.projectId },
                { "TITLE", this.originalProjectId },
                { "COMPILER_LOCALE", this.projectLocale.ID.ToLower() }
            };

            Dictionary<string, FileOutput> output = new Dictionary<string, FileOutput>();
            Dictionary<int, Bitmap> icons = new IconSetGenerator()
                .AddOutputSize(32)
                .AddOutputSize(256)
                .GenerateWithDefaultFallback();
            output["assets/icon32.png"] = new FileOutput() { Type = FileOutputType.Image, Bitmap = icons[32] };
            output["assets/icon256.png"] = new FileOutput() { Type = FileOutputType.Image, Bitmap = icons[256] };

            foreach (string file in new string[]
                {
                    "DefaultProject/BuildFile.txt|%%%PROJECT_ID%%%.build",
                    "DefaultProject/main" + this.projectLocale.ID.ToUpper() + ".txt|source/main.cry",
                    "DefaultProject/dotGitIgnore.txt|output/.gitignore",
                })
            {
                string[] parts = file.Split('|');
                string content = new ResourceStore(typeof(DefaultProjectGenerator)).ReadAssemblyFileText(parts[0]);
                content = this.ReplaceStrings(content);
                string outputPath = this.ReplaceStrings(parts[1]);
                output[outputPath] = new FileOutput()
                {
                    Type = FileOutputType.Text,
                    TextContent = content,
                };
            }
            // TODO: why is this here? Isn't this dead code?
            new ResourceStore(typeof(DefaultProjectGenerator)).ReadAssemblyFileText("DefaultProject/BuildFile.txt");
            return output;
        }

        private string ReplaceStrings(string text)
        {
            return Constants.DoReplacements(false, text, this.replacements);
        }
    }
}
