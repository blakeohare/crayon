using System;
using System.Collections.Generic;
using System.Text;

namespace Wax
{
    public class CbxExtensionService : WaxService
    {
        private string verifiedCbxPath = null;
        private string expectedCbxPath = null;
        private string buildFile = null;

        public bool IsPresent { get; set; }

        internal CbxExtensionService(WaxHub hub, string name) : base(name) {

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

        public override void HandleRequest(Dictionary<string, object> request, Func<Dictionary<string, object>, bool> cb)
        {
            Error[] errors = this.EnsureReady() ?? new Error[0];
            if (errors.Length > 0)
            {
                cb(new Dictionary<string, object>() { { "errors", errors } });
                return;
            }

            Dictionary<string, object> result = this.Hub.AwaitSendRequest("runtime", new Dictionary<string, object>() {
                { "cbxPath", this.verifiedCbxPath },
                { "args", new string[0] },
                { "extArgsJson", Util.JsonUtil.SerializeJson(request) },
                { "showLibStack", true },
                { "realTimePrint", true },
                { "useOutputPrefixes", false },
            });

            cb(result);
        }

        private Error[] EnsureReady()
        {
            if (this.verifiedCbxPath == null)
            {
                Dictionary<string, object> compileRequest = new Command()
                {
                    BuildFilePath = this.buildFile,
                    DefaultProjectId = this.Name,
                }.GetRawData();
                BuildData buildData = new BuildData(this.Hub.AwaitSendRequest("compiler2", compileRequest));

                if (buildData.HasErrors) return buildData.Errors;

                byte[] cbxBytes = CbxFileEncoder.Encode(buildData.CbxBundle);
                System.IO.File.WriteAllBytes(this.expectedCbxPath, cbxBytes);
                this.verifiedCbxPath = this.expectedCbxPath;
            }
            return null;
        }

    }
}
