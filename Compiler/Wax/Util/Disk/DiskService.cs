using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wax.Util.Disk
{
    public class DiskService : WaxService
    {
        public DiskService() : base("disk") { }

        public override async Task<Dictionary<string, object>> HandleRequest(Dictionary<string, object> request)
        {
            if (!request.TryGetValue("cmd", out object commandObj)) return new Dictionary<string, object>() { { "ok", false }, { "error", "No command attribute" } };
            string command = (commandObj + "").Trim().ToLower();
            switch (command)
            {
                default:
                    throw new System.NotImplementedException();
            }

        }
    }
}
