using Common;
using CommonUtil;
using CommonUtil.Resources;
using System;
using System.Collections.Generic;
using Wax;

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

        private string[] inheritanceChain = null;
        public IList<string> InheritanceChain
        {
            get
            {
                if (this.inheritanceChain == null)
                {
                    List<string> chainBuilder = new List<string>();
                    AbstractPlatform walker = this;
                    while (walker != null)
                    {
                        chainBuilder.Add(walker.Name);
                        walker = walker.ParentPlatform;
                    }
                    this.inheritanceChain = chainBuilder.ToArray();
                }
                return this.inheritanceChain;
            }
        }

        private Dictionary<string, object> flattenedCached = null;
        public Dictionary<string, object> GetFlattenedConstantFlags(bool isStandaloneVm)
        {
            if (this.flattenedCached == null)
            {
                this.flattenedCached = this.InheritsFrom != null
                    ? new Dictionary<string, object>(this.PlatformProvider.GetPlatform(this.InheritsFrom).GetFlattenedConstantFlags(isStandaloneVm))
                    : new Dictionary<string, object>();

                IDictionary<string, object> thisPlatform = this.GetConstantFlags();
                foreach (string key in thisPlatform.Keys)
                {
                    this.flattenedCached[key] = thisPlatform[key];
                }
            }

            Dictionary<string, object> output = new Dictionary<string, object>(this.flattenedCached);
            if (!isStandaloneVm &&
                output.ContainsKey("HAS_DEBUGGER") &&
                (bool)output["HAS_DEBUGGER"])
            {
                output["HAS_DEBUGGER"] = false;
            }

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
            byte[] bytes = new ResourceStore(this.GetType()).ReadAssemblyFileBytes(resourcePath);
            if (bytes == null)
            {
                AbstractPlatform parent = this.ParentPlatform;
                if (parent == null)
                {
                    return null;
                }
                return parent.LoadBinaryResource(resourcePath);
            }
            return bytes;
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
            if (content == null)
            {
                AbstractPlatform parent = this.ParentPlatform;
                if (parent == null)
                {
                    throw new Exception("Resource not found: '" + resourcePath + "'");
                }
                return parent.LoadTextResource(resourcePath, replacements);
            }

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

        private bool parentPlatformSet = false;
        private AbstractPlatform parentPlatform = null;
        public AbstractPlatform ParentPlatform
        {
            get
            {
                if (!this.parentPlatformSet)
                {
                    this.parentPlatformSet = true;
                    this.parentPlatform = this.PlatformProvider.GetPlatform(this.InheritsFrom);
                }
                return this.parentPlatform;
            }
        }

        public abstract void ExportStandaloneVm(Dictionary<string, FileOutput> output);

        public abstract void ExportProject(
            Dictionary<string, FileOutput> output,
            BuildData buildData,
            Options options);

        public virtual void TranspileCode(Dictionary<string, FileOutput> output, object parserContextObj)
        {
            throw new NotImplementedException();
        }

        public abstract Dictionary<string, string> GenerateReplacementDictionary(Options options, BuildData buildData);

        protected static Dictionary<string, string> GenerateGeneralReplacementsDictionary(Options options)
        {
            return new Dictionary<string, string>()
            {
                { "PROJECT_ID", options.GetString(ExportOptionKey.PROJECT_ID) },
                { "PROJECT_ID_LOWERCASE", options.GetString(ExportOptionKey.PROJECT_ID).ToLowerInvariant() },
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

        public void GenerateIconFile(Dictionary<string, FileOutput> files, string iconOutputPath, Options options)
        {
            string[] iconPaths = options.GetStringArray(ExportOptionKey.ICON_PATH);
            CommonUtil.Images.IconGenerator iconGen = new CommonUtil.Images.IconGenerator();
            foreach (string path in iconPaths)
            {
                iconGen.AddImage(new CommonUtil.Images.Bitmap(path.Trim()));
            }

            files[iconOutputPath] = new FileOutput()
            {
                Type = FileOutputType.Binary,
                BinaryContent = iconGen.GenerateIconFile(),
            };
        }
    }
}
