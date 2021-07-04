using System;
using System.Collections.Generic;

namespace CommonUtil.Wax
{
    public class WaxHub
    {
        private Dictionary<string, WaxService> services = new Dictionary<string, WaxService>();

        public void RegisterService(WaxService service)
        {
            this.services[service.Name] = service;
        }

        public void SendRequest(
            string serviceName,
            Dictionary<string, object> request,
            Func<Dictionary<string, object>, bool> cb)
        {
            WaxService service = this.services.ContainsKey(serviceName) ? this.services[serviceName] : null;
            if (service == null)
            {
                // TODO: check for extensible services online
                throw new Exception("Invalid service name: '" + serviceName + "'");
            }
            string encodedRequest = WaxService.SerializeWireData(request);
            System.ComponentModel.BackgroundWorker bg = new System.ComponentModel.BackgroundWorker();
            bg.DoWork += (e, sender) =>
            {
                service.HandleRequest(WaxService.ParseWireData(encodedRequest), cb);
            };
            bg.RunWorkerAsync();
        }
    }
}
