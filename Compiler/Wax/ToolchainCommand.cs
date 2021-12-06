using System.Collections.Generic;
using System.Linq;

namespace Wax
{
    public class ToolchainCommand : JsonBasedObject
    {
        public ToolchainCommand() : base() { }
        public ToolchainCommand(IDictionary<string, object> data) : base(data) { }

        private Dictionary<string, string> argCache = null;

        public string[] RuntimeArgs { get { return this.GetStrings("runtimeArgs"); } set { this.SetStrings("runtimeArgs", value); } }
        public ExtensionArg[] ExtensionArgs { get { return this.GetObjectsAsType<ExtensionArg>("extensions"); } set { this.SetObjects("extensions", value); } }
        public ToolchainArg[] ToolchainArgs { get { return this.GetObjectsAsType<ToolchainArg>("toolchainArgs"); } set { this.SetObjects("toolchainArgs", value); this.argCache = null; } }
        public string BuildFile { get { return this.GetString("buildFile"); } set { this.SetString("buildFile", value); } }
        public string CbxFile { get { return this.GetString("cbxFile"); } set { this.SetString("cbxFile", value); } }

        public string[] Extensions
        {
            get
            {
                return (this.ExtensionArgs ?? new ExtensionArg[0])
                    .Select(extArg => extArg.Extension)
                    .Distinct()
                    .OrderBy(name => name)
                    .ToArray();
            }
        }

        private string GetToolchainArg(string name)
        {
            if (this.argCache == null)
            {
                this.argCache = new Dictionary<string, string>();
                foreach (ToolchainArg tca in this.ToolchainArgs ?? new ToolchainArg[0])
                {
                    this.argCache[tca.Name ?? ""] = tca.Value;
                }
            }

            return this.argCache.ContainsKey(name) ? this.argCache[name] : null;
        }

        public bool IsErrorCheckOnly { get { return this.GetToolchainArg("buildErrorCheck") != null; } }
        public bool UseJsonOutput { get { return this.GetToolchainArg("useJsonOutput") != null; } }
        public bool ShowDependencyTree {  get { return this.GetToolchainArg("showDepTree") != null; } }
        public bool UseOutputPrefixes {  get { return this.GetToolchainArg("useOutputPrefixes") != null; } }
        public bool ShowLibraryStackTraces { get { return this.GetToolchainArg("showLibStack") != null; } }

        // These need to be converted into build args
        public string BuildTarget { get { return this.GetToolchainArg("target"); } }
        public string OutputDirectoryOverride { get { return this.GetToolchainArg("outputDirOverride"); } }
        public string CbxExportPath { get { return this.GetToolchainArg("cbxExportPath"); } }
    }

    public class ToolchainArg : JsonBasedObject
    {
        public ToolchainArg() : base() { }
        public ToolchainArg(IDictionary<string, object> data) : base(data) { }

        public string Name { get { return this.GetString("name"); } set { this.SetString("name", value); } }
        public string Value { get { return this.GetString("value"); } set { this.SetString("value", Value); } }
    }

    public class ExtensionArg : JsonBasedObject
    {
        public ExtensionArg() : base() { }
        public ExtensionArg(IDictionary<string, object> data) : base(data) { }

        public string Extension { get { return this.GetString("ext"); } set { this.SetString("ext", value); } }
        public string Name { get { return this.GetString("name"); } set { this.SetString("name", value); } }
        public string Value { get { return this.GetString("value"); } set { this.SetString("value", Value); } }
    }
}
