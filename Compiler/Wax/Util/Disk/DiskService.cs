using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wax.Util.Disk
{
    /*
        Disk types that can be used:
        - Physical disk
        - Sandboxed Physical disk - a designated directory that is treated like its own volume
        - Memory disk - explicitly created and given an ID
        - User data disk - sandboxed disk specific to the given project ID
        - CryPkg - a crayon package or CBX file that can be treated as a read-only directory
    */
    public class DiskService : WaxService
    {
        public DiskService() : base("disk") { }

        private static readonly System.Random rnd = new System.Random();
        private static string GenerateDiskId()
        {
            string chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < 30; i++)
            {
                sb.Append(chars[rnd.Next(chars.Length)]);
            }
            return sb.ToString();
        }

        private string physicalDiskId = GenerateDiskId();

        private Dictionary<string, AbstractDisk> disksById = new Dictionary<string, AbstractDisk>();

        private AbstractDisk GetDisk(string id)
        {
            if (this.disksById.ContainsKey(id)) return this.disksById[id];
            return null;
        }

        private static string GetStringArg(Dictionary<string, object> request, string argName)
        {
            return (GetRequestArg(request, argName) ?? "") + "";
        }

        private static string GetCanonicalStringArg(Dictionary<string, object> request, string argName)
        {
            return GetStringArg(request, argName).Trim().ToLower();
        }

        private static object GetRequestArg(Dictionary<string, object> request, string argName)
        {
            if (request.ContainsKey(argName)) return request[argName];
            return null;
        }

        private static Dictionary<string, object> BuildError(string errorCode, string message)
        {
            return new Dictionary<string, object>() {
                { "ok", false },
                { "code", errorCode },
                { "error", message },
            };
        }

        public override async Task<Dictionary<string, object>> HandleRequest(Dictionary<string, object> request)
        {
            string command = GetCanonicalStringArg(request, "cmd");
            if (command == "") return BuildError("INVALID_ARG", "No command attribute");

            string diskId = GetStringArg(request, "diskId").Trim();

            AbstractDisk disk = diskId == "" ? null : this.GetDisk(diskId);
            if (diskId != "" && disk == null)
            {
                return BuildError("DISK_NOT_FOUND", "That disk could not be found");
            }

            switch (command)
            {
                case "allocate":
                    switch (GetCanonicalStringArg(request, "type"))
                    {
                        case "physical":
                            // No distinction between multiple actual physical disks. The disk service only sees the "physical realm" as a disk. Distinction between
                            // multiple disks is at the mercy of the actual absolute paths used.
                            if (this.GetDisk(this.physicalDiskId) == null)
                            {
                                this.disksById[this.physicalDiskId] = new PhysicalDisk(this.physicalDiskId);
                            }
                            return new Dictionary<string, object>() { { "ok", true }, { "diskId", physicalDiskId } };
                        default:
                            throw new System.NotImplementedException();
                    }

                case "filereadtext":
                    {
                        string path = GetStringArg(request, "path");
                        if (path == "") return BuildError("INVALID_ARG", "FileReadText command is missing a path");
                        string value = await disk.FileReadText(path);
                        if (value == null) return BuildError("FILE_NOT_FOUND", "File not found");
                        return new Dictionary<string, object>() { { "ok", true }, { "content", value } };
                    }

                default:
                    throw new System.NotImplementedException();
            }
        }
    }
}
