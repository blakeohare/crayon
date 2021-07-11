using System;
using System.Collections.Generic;

namespace Extensions
{
    internal class LanguageFrontendService : CommonUtil.Wax.WaxService
    {
        private string language;
        private string cbxFolderId;

        public LanguageFrontendService(string lang, string folderId) : base("langfe-" + lang)
        {
            this.language = lang;
            this.cbxFolderId = folderId;
        }

        public override void HandleRequest(Dictionary<string, object> request, Func<Dictionary<string, object>, bool> cb)
        {
            Dictionary<string, object> result = this.Hub.AwaitSendRequest("cbxrunner", new Dictionary<string, object>() { });

            throw new NotImplementedException();

            // cb(new Dictionary<string, object>() { });
        }
    }
}
