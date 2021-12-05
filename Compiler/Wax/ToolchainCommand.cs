using System.Collections.Generic;

namespace Wax
{
    public class ToolchainCommand : JsonBasedObject
    {
        public ToolchainCommand() : base() { }
        public ToolchainCommand(IDictionary<string, object> data) : base(data) { }

        public string[] RuntimeArgs { get { return this.GetStrings("runtimeArgs"); } set { this.SetStrings("runtimeArgs", value); } }
        public ExtensionArg[] ExtensionArgs { get { return this.GetObjectsAsType<ExtensionArg>("extensions"); } set { this.SetObjects("extensions", value); } }
        public ToolchainArg[] ToolchainArgs { get { return this.GetObjectsAsType<ToolchainArg>("toolchainArgs"); } set { this.SetObjects("toolchainArgs", value); } }
        public string BuildFile {  get { return this.GetString("buildFile"); } set { this.SetString("buildFile", value); } }
        public string CbxFile { get { return this.GetString("cbxFile"); } set { this.SetString("cbxFile", value); } }
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
