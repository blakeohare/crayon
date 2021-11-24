using Common;
using CommonUtil;
using CommonUtil.Disk;
using CommonUtil.Images;
using CommonUtil.Resources;
using System;
using System.Collections.Generic;
using Wax;

namespace Crayon
{
    public class DefaultProjectGenerator
    {
        public void DoWorkImpl(string projectId, string projectLocale, string projectType)
        {
            DefaultProjectGeneratorImpl generator = new DefaultProjectGeneratorImpl(projectId, projectLocale);
            Dictionary<string, FileOutput> project = generator.Validate().Export(projectType ?? "basic");

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
            private Parser.Localization.Locale projectLocale;

            public string ProjectID { get; private set; }

            public DefaultProjectGeneratorImpl(string projectId, string localeId)
            {
                this.originalProjectId = projectId.Trim();
                this.projectLocale = Parser.Localization.Locale.Get(localeId);
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

            public Dictionary<string, FileOutput> Export(string projectType)
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

                List<string> files = new List<string>() {
                    "DefaultProjects/BuildFile.txt|%%%PROJECT_ID%%%.build",
                    "DefaultProjects/dotGitIgnore.txt|.gitignore",
                };
                switch (projectType)
                {
                    case "basic":
                        files.Add("DefaultProjects/main" + this.projectLocale.ID.ToUpperInvariant() + ".txt|source/main.cry");
                        break;

                    case "game":
                        files.Add("DefaultProjects/game.txt|source/main.cry");
                        break;

                    case "ui":
                        files.Add("DefaultProjects/ui.txt|source/main.cry");
                        break;

                    default:
                        throw new InvalidOperationException("Unknown project type: '" + projectType + "'");
                }

                foreach (string file in files)
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

                return output;
            }

            private string ReplaceStrings(string text)
            {
                return ReplaceStringsImpl(false, text, this.replacements);
            }

            private static string ReplaceStringsImpl(bool keepPercents, string text, Dictionary<string, string> replacements)
            {
                if (keepPercents) return text;

                if (text.Contains("%%%"))
                {
                    string[] parts = StringUtil.Split(text, "%%%");
                    bool lastWasReplacement = false;

                    List<string> replaced = new List<string>() { parts[0] };
                    int i = 1;
                    for (; i < parts.Length - 1; ++i)
                    {
                        string key = parts[i];
                        if (replacements.ContainsKey(key))
                        {
                            replaced.Add(replacements[key]);
                            replaced.Add(parts[++i]);
                            lastWasReplacement = true;
                        }
                        else
                        {
                            replaced.Add("%%%");
                            replaced.Add(key);
                            lastWasReplacement = false;
                        }
                    }

                    if (!lastWasReplacement)
                    {
                        replaced.Add("%%%");
                        replaced.Add(parts[parts.Length - 1]);
                    }

                    text = string.Join("", replaced);
                }

                return text;
            }
        }
    }
}
