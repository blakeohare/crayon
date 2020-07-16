using Build;
using Common;
using CommonUtil;
using CommonUtil.Resources;
using System;
using System.Collections.Generic;

namespace Platform
{
    public abstract class AbstractPlatform
    {
        public AbstractPlatform(string language)
        {
            this.Language = language;
        }

        public IPlatformProvider PlatformProvider { get; set; }

        public string Language { get; private set; }
        public abstract string Name { get; }
        public abstract string InheritsFrom { get; }

        public abstract IDictionary<string, object> GetConstantFlags();
        public abstract string NL { get; }
        protected int TranslationIndentionCount { get; set; }

        private Dictionary<string, object> flattenedCached = null;
        public Dictionary<string, object> GetFlattenedConstantFlags()
        {
            if (this.flattenedCached == null)
            {
                this.flattenedCached = new Dictionary<string, object>(this.GetConstantFlags());
            }

            Dictionary<string, object> output = new Dictionary<string, object>(this.flattenedCached);

            return output;
        }

        public void CopyResourceAsBinary(Dictionary<string, FileOutput> output, string outputPath, string resourcePath)
        {
            output[outputPath] = new FileOutput()
            {
                Type = FileOutputType.Binary,
                BinaryContent = this.LoadBinaryResource(resourcePath),
            };
        }

        public byte[] LoadBinaryResource(string resourcePath)
        {
            return new ResourceStore(this.GetType()).ReadAssemblyFileBytes(resourcePath);
        }

        public void CopyResourceAsText(Dictionary<string, FileOutput> output, string outputPath, string resourcePath, Dictionary<string, string> replacements)
        {
            output[outputPath] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = this.LoadTextResource(resourcePath, replacements),
            };
        }

        public string LoadTextResource(string resourcePath, Dictionary<string, string> replacements)
        {
            string content = new ResourceStore(this.GetType()).ReadAssemblyFileText(resourcePath, true);
            return this.ApplyReplacements(content, replacements);
        }

        protected string ApplyReplacements(string text, Dictionary<string, string> replacements)
        {
            if (text.Contains("%%%"))
            {
                string[] segments = StringUtil.Split(text, "%%%");
                for (int i = 1; i < segments.Length; i += 2)
                {
                    string replacement;
                    if (replacements.TryGetValue(segments[i], out replacement))
                    {
                        segments[i] = replacement;
                    }
                    else
                    {
                        segments[i] = "%%%" + segments[i] + "%%%";
                    }
                }
                text = string.Join("", segments);
            }
            return text;
        }

        public abstract void ExportStandaloneVm(
            Dictionary<string, FileOutput> output,
            IList<LibraryForExport> everyLibrary);

        public abstract Dictionary<string, string> GenerateReplacementDictionary(Options options, ResourceDatabase resDb);

        protected static Dictionary<string, string> GenerateGeneralReplacementsDictionary(Options options)
        {
            return new Dictionary<string, string>()
            {
                { "PROJECT_ID", options.GetString(ExportOptionKey.PROJECT_ID) },
                { "PROJECT_TITLE", options.GetString(ExportOptionKey.PROJECT_TITLE, "Untitled Project") },
                { "PROJECT_DESCRIPTION", options.GetStringOrEmpty(ExportOptionKey.DESCRIPTION) },
                { "PROJECT_VERSION", options.GetStringOrEmpty(ExportOptionKey.VERSION) },
                { "DEFAULT_WINDOW_WIDTH", options.GetInteger(ExportOptionKey.WINDOW_WIDTH, 800).ToString() },
                { "DEFAULT_WINDOW_HEIGHT", options.GetInteger(ExportOptionKey.WINDOW_HEIGHT, 600).ToString() },
            };
        }

        public virtual void GleanInformationFromPreviouslyExportedProject(
            Options options,
            string outputDirectory)
        { }
    }
}
