using System;
using System.Collections.Generic;
using System.Linq;

namespace CommonUtil.Wax
{
    public class WaxHub
    {
        private Dictionary<string, WaxService> services = new Dictionary<string, WaxService>();

        public void RegisterService(WaxService service)
        {
            service.Hub = this;
            this.services[service.Name] = service;
        }

        public void SendRequest(
            string serviceName,
            Dictionary<string, object> request,
            Func<Dictionary<string, object>, bool> cb)
        {
            if (cb == null) cb = d => false;

            WaxService service = this.services.ContainsKey(serviceName) ? this.services[serviceName] : null;
            if (service == null)
            {
                // TODO: check for extensible services online
                throw new Exception("Invalid service name: '" + serviceName + "'");
            }
            string encodedRequest = SerializeWireData(request);
            System.ComponentModel.BackgroundWorker bg = new System.ComponentModel.BackgroundWorker();
            bg.DoWork += (e, sender) =>
            {
                service.HandleRequest(ParseWireData(encodedRequest), cb);
            };
            bg.RunWorkerAsync();
        }

        public Dictionary<string, object> AwaitSendRequest(
            string serviceName,
            Dictionary<string, object> request)
        {
            List<Dictionary<string, object>> responsePtr = new List<Dictionary<string, object>>();
#if DEBUG
            Dictionary<string, object> immutableEnsuredCopy = ParseWireData(SerializeWireData(request));
            this.services[serviceName].HandleRequest(
                immutableEnsuredCopy,
                response =>
                {
                    responsePtr.Add(response);
                    return true;
                });

            if (responsePtr.Count == 0) throw new NotImplementedException();
            return responsePtr[0];
#else

            this.SendRequest(serviceName, request, response =>
            {
                lock (responsePtr)
                {
                    responsePtr.Add(response);
                }
                return true;
            });

            double counter = 1;
            while (true)
            {
                lock (responsePtr)
                {
                    if (responsePtr.Count > 0)
                    {
                        return responsePtr[0];
                    }
                }
                System.Threading.Thread.Sleep((int)counter);
                counter *= 1.5;
                if (counter > 100) counter = 100;
            }
#endif
        }


        internal static string SerializeWireData(Dictionary<string, object> data)
        {
            List<string> wireData = new List<string>();
            foreach (string key in data.Keys)
            {
                object value = data[key];
                wireData.Add(key);
                if (value == null)
                {
                    wireData.Add("N");
                    wireData.Add("");
                }
                else if (value is int)
                {
                    wireData.Add("I");
                    wireData.Add("" + data);
                }
                else if (value is bool)
                {
                    wireData.Add("B");
                    wireData.Add((bool)value ? "1" : "0");
                }
                else if (value is string)
                {
                    wireData.Add("S");
                    wireData.Add((string)value);
                }
                else if (value is string[])
                {
                    wireData.Add("A");
                    string[] items = (string[])value;
                    wireData.Add(items.Length + "");
                    foreach (string item in items)
                    {
                        wireData.Add(item);
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            return string.Join(',', wireData.Select(item => Base64.ToBase64(item)));
        }

        internal static Dictionary<string, object> ParseWireData(string encodedData)
        {
            Dictionary<string, object> output = new Dictionary<string, object>();
            string[] items = encodedData.Split(',').Select(item => Base64.FromBase64(item)).ToArray();
            for (int i = 0; i < items.Length; i += 3)
            {
                string key = items[i];
                char type = items[i + 1][0];
                string data = items[i + 2];
                switch (type)
                {
                    case 'B': output[key] = data == "1"; break;
                    case 'I': output[key] = int.Parse(data); break;
                    case 'S': output[key] = data; break;
                    case 'N': output[key] = null; break;
                    case 'A':
                        List<string> arr = new List<string>();
                        int length = int.Parse(data);
                        for (int j = 0; j < length; ++j)
                        {
                            arr.Add(items[i++ + 3]);
                        }
                        output[key] = arr.ToArray();
                        break;

                    default: throw new NotImplementedException();
                }
            }
            return output;
        }
    }
}
