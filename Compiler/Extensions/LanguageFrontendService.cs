using System;
using System.Collections.Generic;

namespace Extensions
{
    internal class LanguageFrontendService : CommonUtil.Wax.WaxService
    {
        private string language;
        private string cbxPath;

        public LanguageFrontendService(string lang, string cbxPath) : base("langfe-" + lang)
        {
            this.language = lang;
            this.cbxPath = cbxPath;
        }

        public override void HandleRequest(Dictionary<string, object> request, Func<Dictionary<string, object>, bool> cb)
        {
            string sourceDirectory = (string)request["sourceDir"];
            string targetDirectory = (string)request["targetDir"];
            string entryPointFile = (string)request["entryPointFile"];
            Dictionary<string, object> result = this.Hub.AwaitSendRequest("cbxrunner", new Dictionary<string, object>() {
                { "cbxPath", this.cbxPath },
                { "realTimePrint", false },
                { "showLibStack", true },
                { "useOutputPrefixes", true },
                { "args", new string[] { sourceDirectory, entryPointFile, targetDirectory } },
            });

            cb(new Dictionary<string, object>() {
                { "done", true },
            });
        }
    }
}
