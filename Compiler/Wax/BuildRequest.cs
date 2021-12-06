using System.Collections.Generic;

namespace Wax
{
    public class BuildRequest : JsonBasedObject
    {
        public BuildRequest() : base() { }
        public BuildRequest(IDictionary<string, object> data) : base(data) { }

        public string BuildFile { get { return this.GetString("buildFile"); } set { this.SetString("buildFile", value); } }
        public string BuildTarget { get { return this.GetString("target"); } set { this.SetString("target", value); } }
        public string OutputDirectoryOverride { get { return this.GetString("outputDirOverride"); } set { this.SetString("outputDirOverride", value); } }
        public bool IsErrorCheck { get { return this.GetBoolean("isErrorCheck"); } set { this.SetBoolean("isErrorCheck", value); } }
    }
}
