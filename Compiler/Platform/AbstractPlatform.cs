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


        public string Language { get; private set; }
        public abstract string Name { get; }

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
                this.flattenedCached = this.ParentPlatform != null
                    ? new Dictionary<string, object>(this.ParentPlatform.GetFlattenedConstantFlags(isStandaloneVm))
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

        public AbstractPlatform ParentPlatform { get; set; }

        public abstract void ExportProject(
            Dictionary<string, FileOutput> output,
            BuildData buildData,
            ExportProperties exportProperties);

        public virtual void TranspileCode(Dictionary<string, FileOutput> output, object parserContextObj)
        {
            throw new NotImplementedException();
        }

        public abstract Dictionary<string, string> GenerateReplacementDictionary(ExportProperties exportProperties, BuildData buildData);

        protected static Dictionary<string, string> GenerateGeneralReplacementsDictionary(ExportProperties exportProperties)
        {
            return new Dictionary<string, string>()
            {
                { "PROJECT_ID", exportProperties.ProjectID },
                { "PROJECT_ID_LOWERCASE", exportProperties.ProjectID.ToLowerInvariant() },
                { "PROJECT_TITLE", exportProperties.ProjectTitle ?? "Untitled Project" },
                { "PROJECT_DESCRIPTION", exportProperties.Description ?? "" },
                { "PROJECT_VERSION", exportProperties.Version ?? "" },
                { "DEFAULT_WINDOW_WIDTH", "800" },
                { "DEFAULT_WINDOW_HEIGHT", "600" },
            };
        }

        public virtual void GleanInformationFromPreviouslyExportedProject(
            ExportProperties exportProperties,
            string outputDirectory)
        { }

        public void GenerateIconFile(Dictionary<string, FileOutput> files, string iconOutputPath, ExportProperties exportProperties)
        {
            string[] iconPaths = exportProperties.IconPaths;
            Wax.Util.Images.IconGenerator iconGen = new Wax.Util.Images.IconGenerator();
            foreach (string path in iconPaths)
            {
                iconGen.AddImage(new Wax.Util.Images.Bitmap(path.Trim()));
            }

            files[iconOutputPath] = new FileOutput()
            {
                Type = FileOutputType.Binary,
                BinaryContent = iconGen.GenerateIconFile(),
            };
        }
    }
}
