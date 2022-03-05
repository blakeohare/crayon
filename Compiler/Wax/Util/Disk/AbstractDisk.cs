using System.Threading.Tasks;

namespace Wax.Util.Disk
{
    internal abstract class AbstractDisk
    {
        public string ID { get; private set; }

        public AbstractDisk(string id)
        {
            this.ID = id;
        }

        public abstract Task<string> FileReadText(string path);
    }

    internal class PhysicalDisk : AbstractDisk
    {
        public PhysicalDisk(string id) : base(id)
        { }

        private static string NormalizePath(string dir)
        {
            dir = dir.Trim().Replace('\\', '/').TrimEnd('/');
            if (dir.Length == 0) dir = "/";
            return dir.Replace('/', System.IO.Path.DirectorySeparatorChar);
        }

        public override Task<string> FileReadText(string path)
        {
            path = NormalizePath(path);
            if (System.IO.File.Exists(path))
            {
                path = NormalizePath(path);
                byte[] bytes = System.IO.File.ReadAllBytes(path);
                string text = UniversalTextDecoder.Decode(bytes);
                return Task.FromResult(text);
            }
            return Task.FromResult<string>(null);
        }
    }
}
