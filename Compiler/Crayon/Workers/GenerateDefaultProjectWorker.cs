using Common;
using CommonUtil;
using CommonUtil.Disk;
using CommonUtil.Images;
using CommonUtil.Resources;
using Exporter;
using System;
using System.Collections.Generic;

namespace Crayon
{
    public class DefaultProjectGenerator
    {
        public void DoWorkImpl(string projectId, string projectLocale)
        {
            DefaultProjectGeneratorImpl generator = new DefaultProjectGeneratorImpl(projectId, projectLocale);
            Dictionary<string, FileOutput> project = generator.Validate().Export();

            string directory = FileUtil.JoinPath(
                Path.GetCurrentDirectory(),
                generator.ProjectID);
            new FileOutputExporter(directory).ExportFiles(project);

            ConsoleWriter.Print(
                ConsoleMessageType.DEFAULT_PROJ_EXPORT_INFO,
                "Empty project exported to directory '" + generator.ProjectID + "/'");
        }

        private class DefaultProjectGeneratorImpl
        {
            private string originalProjectId;
            private Localization.Locale projectLocale;

            public string ProjectID { get; private set; }

            public DefaultProjectGeneratorImpl(string projectId, string localeId)
            {
                this.originalProjectId = projectId.Trim();
                this.projectLocale = Localization.Locale.Get(localeId);
                this.ProjectID = StringUtil.FilterStringToAlphanumerics(this.originalProjectId);
            }

            public DefaultProjectGeneratorImpl Validate()
            {
                if (this.ProjectID.Length == 0) throw new InvalidOperationException("Project name did not have any alphanumeric characters.");
                if (this.ProjectID[0] >= '0' && this.ProjectID[0] <= '9') throw new InvalidOperationException("Project name cannot begin with a number.");
                if (this.ProjectID != this.originalProjectId)
                {
                    ConsoleWriter.Print(
                        ConsoleMessageType.BUILD_WARNING,
                        "Warning: '" + this.originalProjectId + "' contains non-alphanumeric characters and was canonicalized into '" + this.ProjectID + "'");
                }

                return this;
            }

            private Dictionary<string, string> replacements;

            public Dictionary<string, FileOutput> Export()
            {
                this.replacements = new Dictionary<string, string>()
                {
                    { "PROJECT_ID", this.ProjectID },
                    { "TITLE", this.originalProjectId },
                    { "COMPILER_LOCALE", this.projectLocale.ID.ToLowerInvariant() }
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
                        "DefaultProject/main" + this.projectLocale.ID.ToUpperInvariant() + ".txt|source/main.cry",
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
                return ConstantReplacer.DoReplacements(false, text, this.replacements);
            }
        }
    }
}
