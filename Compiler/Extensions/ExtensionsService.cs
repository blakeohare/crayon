using System;
using System.Collections.Generic;

namespace Extensions
{
    public class ExtensionsService : CommonUtil.Wax.WaxService
    {

        private HashSet<string> loadedServices = new HashSet<string>();

        public ExtensionsService() : base("extensions") { }

        public override void HandleRequest(Dictionary<string, object> request, Func<Dictionary<string, object>, bool> cb)
        {
            Dictionary<string, object> result = null;
            switch ((string)request["command"])
            {
                case "getLibrary":
                    throw new NotImplementedException();

                case "getExporter":
                    throw new NotImplementedException();

                case "getLanguageFrontend":
                    result = this.GetLangFrontend((string)request["lang"]);
                    break;
            }
            cb(result);
        }

        private Dictionary<string, object> GetLangFrontend(string lang)
        {
            if (this.loadedServices.Contains("langfe-" + lang))
            {
                return new Dictionary<string, object>() {
                    { "ready", true },
                };
            }

            Dictionary<string, object> result = this.Hub.AwaitSendRequest("disk", new Dictionary<string, object>() {
                { "command", "allocFolderIdIfExists" },
                { "path", "langfe/" + lang },
                { "type", "appdata" },
            });

            if (!(bool)result["found"])
            {
                throw new NotImplementedException(); // extension not found
            }

            return new Dictionary<string, object>() {
                { "ready", true },
            };
        }
    }
}
