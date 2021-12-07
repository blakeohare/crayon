using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wax
{
    public abstract class WaxService
    {
        public WaxService(string name)
        {
            this.Name = name;
        }

        public string Name { get; set; }
        public WaxHub Hub { get; set; }
        public abstract Task<Dictionary<string, object>> HandleRequest(Dictionary<string, object> request);

        public Task<Dictionary<string, object>> SendRequest(string serviceName, Dictionary<string, object> request)
        {
            return this.Hub.SendRequest(serviceName, request);
        }
    }
}
