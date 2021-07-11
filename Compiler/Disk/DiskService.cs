using System;
using System.Collections.Generic;

namespace Disk
{
    public class DiskService : CommonUtil.Wax.WaxService
    {

        private Dictionary<string, string> idToAbsolutePath = new Dictionary<string, string>();

        public DiskService() : base("disk") { }

        public override void HandleRequest(Dictionary<string, object> request, Func<Dictionary<string, object>, bool> cb)
        {
            Dictionary<string, object> result;
            switch ((string)request["command"])
            {
                case "designateDisk":

                    string realPath = (string)request["realPath"];
                    string intent = (string)request["intent"];

                    throw new NotImplementedException();

                case "allocFolderIdIfExists":
                    result = this.AllocateFolderIdIfExists((string)request["type"], (string)request["path"]);
                    break;

                default:
                    throw new InvalidOperationException();
            }

            cb(result);
        }

        private Dictionary<string, object> AllocateFolderIdIfExists(string type, string path)
        {
            string absDir = this.GetAbsolutePath(type, path);
            if (System.IO.Directory.Exists(absDir))
            {
                string id = Util.GenerateId(12);
                this.idToAbsolutePath[id] = absDir;
                return new Dictionary<string, object>() {
                    { "found", true },
                };

            }
            return new Dictionary<string, object>() {
                { "found", false },
            };
        }

        private string GetAbsolutePath(string type, string path)
        {
            switch (type)
            {
                case "appdata":
                    char sep = System.IO.Path.DirectorySeparatorChar;
                    string appDataFolder = System.Environment.GetEnvironmentVariable("APPDATA");
                    return System.IO.Path.Combine(appDataFolder, "Crayon", path.Replace('/', sep).Replace('\\', sep));
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
