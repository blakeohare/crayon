using System;
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
        public virtual void RegisterListener(Dictionary<string, object> request, Func<Dictionary<string, object>, bool> callback) { }
    }
}
