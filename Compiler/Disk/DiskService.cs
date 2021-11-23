using System;
using System.Collections.Generic;

namespace Disk
{
    public class DiskService : Wax.WaxService
    {

        private Dictionary<string, string> idToAbsolutePath = new Dictionary<string, string>();

        public DiskService() : base("disk") { }

        public override void HandleRequest(Dictionary<string, object> request, Func<Dictionary<string, object>, bool> cb)
        {
            Dictionary<string, object> result;
            switch ((string)request["command"])
            {
                case "resolvePath":
                    string resolvedPath = this.ResolvePath((string)request["path"]);
                    result = new Dictionary<string, object>() {
                        { "path", resolvedPath },
                    };
                    break;

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

        private string ResolvePath(string rawPath)
        {
            string path = rawPath;
            int varIndex = 1;
            string token = "{DISK:";
            while (varIndex != -1)
            {
                varIndex = path.IndexOf("{DISK:");
                if (varIndex >= 0)
                {
                    string left = path.Substring(0, varIndex);
                    string right = path.Substring(varIndex + token.Length);
                    int closeLoc = right.IndexOf('}');
                    if (closeLoc == -1)
                    {
                        varIndex = -1;
                    }
                    else
                    {
                        string folderId = right.Substring(0, closeLoc);
                        right = right.Substring(closeLoc + 1);
                        if (this.idToAbsolutePath.ContainsKey(folderId))
                        {
                            path = left + this.idToAbsolutePath[folderId] + right;
                        }
                        else
                        {
                            varIndex = -1;
                        }
                    }
                }
            }

            return path.Replace('/', System.IO.Path.DirectorySeparatorChar);
        }

        private Dictionary<string, object> AllocateFolderIdIfExists(string type, string path)
        {
            string absDir = this.GetAbsolutePath(type, path);
            if (System.IO.Directory.Exists(absDir))
            {
                string id = this.idToAbsolutePath.ContainsKey(absDir)
                    ? this.idToAbsolutePath[absDir]
                    : Util.GenerateId(12);
                this.idToAbsolutePath[id] = absDir;
                return new Dictionary<string, object>() {
                    { "folderId", id },
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
