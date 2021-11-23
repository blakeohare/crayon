using System;
using System.Collections.Generic;

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
        public abstract void HandleRequest(Dictionary<string, object> request, Func<Dictionary<string, object>, bool> cb);

        public void SendRequest(string serviceName, Dictionary<string, object> request, Func<Dictionary<string, object>, bool> cb)
        {
            this.Hub.SendRequest(serviceName, request, cb);
        }

        public Dictionary<string, object> AwaitSendRequest(string serviceName, Dictionary<string, object> request)
        {
            return this.Hub.AwaitSendRequest(serviceName, request);
        }
    }
}
