using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wax
{
    public class CbxExtensionService : WaxService
    {
        private string verifiedCbxPath = null;
        private string expectedCbxPath = null;
        private string buildFile = null;

        public bool IsPresent { get; set; }

        internal CbxExtensionService(WaxHub hub, string name) : base(name)
        {
            this.Hub = hub;

            foreach (string extensionDirectory in hub.ExtensionDirectories)
            {
                string cbxPath = System.IO.Path.Combine(extensionDirectory, name + ".cbx");
                string buildPath = System.IO.Path.Combine(extensionDirectory, name, name + ".build");

                if (System.IO.File.Exists(buildPath))
                {
                    this.expectedCbxPath = cbxPath;
                    this.buildFile = buildPath;
                    break;
                }

                if (System.IO.File.Exists(cbxPath))
                {
                    this.verifiedCbxPath = cbxPath;
                    break;
                }
            }

            this.IsPresent = this.buildFile != null || this.verifiedCbxPath != null;

            if (!this.IsPresent)
            {
                // TODO: download the extension.
            }
        }

        public override async Task<Dictionary<string, object>> HandleRequest(Dictionary<string, object> request)
        {
            Error[] errors = await this.EnsureReady() ?? new Error[0];
            if (errors.Length > 0)
            {
                return new Dictionary<string, object>() { { "errors", errors } };
            }

            return await this.Hub.SendRequest("runtime", new Dictionary<string, object>() {
                { "cbxPath", this.verifiedCbxPath },
                { "args", new string[0] },
                { "extArgsJson", Util.JsonUtil.SerializeJson(request) },
                { "showLibStack", true },
                { "realTimePrint", true },
                { "useOutputPrefixes", false },
            });
        }

        private async Task<Error[]> EnsureReady()
        {
            if (this.verifiedCbxPath == null)
            {
                BuildRequest buildRequest = new BuildRequest() { BuildFile = this.buildFile };
                BuildData buildData = new BuildData(await this.Hub.SendRequest("builder", buildRequest));

                if (buildData.HasErrors) return buildData.Errors;

                byte[] cbxBytes = CbxFileEncoder.Encode(buildData.CbxBundle);
                System.IO.File.WriteAllBytes(this.expectedCbxPath, cbxBytes);
                this.verifiedCbxPath = this.expectedCbxPath;
            }
            return new Error[0];
        }
    }
}
