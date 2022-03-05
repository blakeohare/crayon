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

        public override Task<string> FileReadText(string path)
        {
            if (System.IO.File.Exists(path))
            {
                return Task.FromResult(System.IO.File.ReadAllText(path));
            }
            return Task.FromResult<string>(null);
        }
    }
}
