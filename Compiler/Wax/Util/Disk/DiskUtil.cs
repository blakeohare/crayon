using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wax.Util.Disk
{
    public class DiskUtil
    {
        private WaxHub hub;
        private string diskId = null;

        public DiskUtil(WaxHub hub)
        {
            this.hub = hub;
        }

        private void EnsureInitialized()
        {
            if (this.diskId == null) throw new System.InvalidOperationException("DiskUtil used before initialized");
        }

        public Task InitializePhysicalDisk()
        {
            return this.Initialize("physical", "");
        }

        private async Task Initialize(string diskType, string diskId)
        {
            Dictionary<string, object> result = await this.hub.SendRequest("disk", new Dictionary<string, object>() {
                { "diskId", diskId },
                { "type", diskType },
                { "cmd", "allocate" },
            });

            bool ok = (bool)result["ok"];
            if (!ok) throw new System.Exception();
            this.diskId = (string)result["diskId"];
        }

        public static string JoinPathCanonical(params string[] args)
        {
            return string.Join('/', args);
        }

        public static string JoinPathNative(params string[] args)
        {
            return string.Join(System.IO.Path.DirectorySeparatorChar, args);
        }

        public static string GetFileName(string path)
        {
            string[] parts = path.Split('/');
            string part = parts[parts.Length - 1];
            parts = part.Split('\\');
            part = parts[parts.Length - 1];
            return part;
        }

        public static string GetCurrentDirectory()
        {
            return System.IO.Directory.GetCurrentDirectory();
        }

        public static bool IsAbsolute(string path)
        {
            if (PlatformUtil.IsWindows)
            {
                if (path.Length > 1 && path[1] == ':') return true;
            }
            else
            {
                if (path.StartsWith("/")) return true;
                if (path.StartsWith("~")) return true;
            }
            return false;
        }

        public async Task<string> FileReadText(string path)
        {
            this.EnsureInitialized();

            Dictionary<string, object> result = await this.hub.SendRequest("disk", new Dictionary<string, object>() {
                { "diskId", this.diskId },
                { "cmd", "fileReadText" },
                { "path", path },
            });

            if (!(bool)result["ok"]) throw new System.NotImplementedException();
            return (string)result["content"];
        }
    }
}
