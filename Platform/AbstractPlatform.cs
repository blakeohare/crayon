using System;
using System.Collections.Generic;
using Common;
using Pastel;

namespace Platform
{
    public abstract class AbstractPlatform
    {
        public IPlatformProvider PlatformProvider { get; set; }

        public abstract string Name { get; }
        public abstract string InheritsFrom { get; }
        public AbstractTranslator Translator { get; protected set; }

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
        public Dictionary<string, object> GetFlattenedConstantFlags()
        {
            if (this.flattenedCached == null)
            {
                this.flattenedCached = this.InheritsFrom != null
                    ? new Dictionary<string, object>(this.PlatformProvider.GetPlatform(this.InheritsFrom).GetFlattenedConstantFlags())
                    : new Dictionary<string, object>();

                IDictionary<string, object> thisPlatform = this.GetConstantFlags();
                foreach (string key in thisPlatform.Keys)
                {
                    this.flattenedCached[key] = thisPlatform[key];
                }
            }

            return new Dictionary<string, object>(this.flattenedCached);
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
            byte[] bytes = Util.ReadAssemblyFileBytes(this.GetType().Assembly, resourcePath);
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
            string content = Util.ReadAssemblyFileText(this.GetType().Assembly, resourcePath, true);
            if (content == null)
            {
                AbstractPlatform parent = this.ParentPlatform;
                if (parent == null)
                {
                    return null;
                }
                return parent.LoadTextResource(resourcePath, replacements);
            }

            if (content.Contains("%%%"))
            {
                foreach (string key in replacements.Keys)
                {
                    content = content.Replace("%%%" + key + "%%%", replacements[key]);
                }
            }
            return content;
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

        public virtual string TranslateType(Pastel.Nodes.PType type)
        {
            if (this.parentPlatformSet)
            {
                return this.parentPlatform.TranslateType(type);
            }
            throw new InvalidOperationException("This platform does not support types.");
        }

        public abstract Dictionary<string, FileOutput> ExportProject(
            IList<Pastel.Nodes.VariableDeclaration> globals,
            IList<Pastel.Nodes.StructDefinition> structDefinitions,
            IList<Pastel.Nodes.FunctionDefinition> functionDefinitions,
            IList<LibraryForExport> libraries,
            ResourceDatabase resourceDatabase,
            Options options,
            ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform);

        public string IndentCodeWithTabs(string code, int tabCount)
        {
            return IndentCodeImpl(code, Util.MultiplyString("\t", tabCount));
        }

        public string IndentCodeWithSpaces(string code, int spaceCount)
        {
            return IndentCodeImpl(code, Util.MultiplyString(" ", spaceCount));
        }

        private static string IndentCodeImpl(string code, string tabSequence)
        {
            string newline = code.Contains("\r\n") ? "\r\n" : "\n";
            string[] lines = code.Split('\n');
            for (int i = 0; i < lines.Length; ++i)
            {
                lines[i] = tabSequence + lines[i].TrimEnd();
            }
            return string.Join(newline, lines);
        }

        public abstract string GenerateCodeForStruct(Pastel.Nodes.StructDefinition structDef);
        public abstract string GenerateCodeForFunction(AbstractTranslator translator, Pastel.Nodes.FunctionDefinition funcDef);
        public abstract string GenerateCodeForGlobalsDefinitions(AbstractTranslator translator, IList<Pastel.Nodes.VariableDeclaration> globals);

        public abstract Dictionary<string, string> GenerateReplacementDictionary(Options options, ResourceDatabase resDb);

        protected static Dictionary<string, string> GenerateGeneralReplacementsDictionary(Options options)
        {
            return new Dictionary<string, string>()
            {
                { "PROJECT_ID", options.GetString(ExportOptionKey.PROJECT_ID) },
            };
        }
    }
}
