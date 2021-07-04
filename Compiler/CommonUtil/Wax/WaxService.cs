using System;
using System.Collections.Generic;
using System.Linq;

namespace CommonUtil.Wax
{
    public abstract class WaxService
    {
        public string Name { get; set; }
        internal WaxHub Hub { get; set; }
        public abstract void HandleRequest(Dictionary<string, object> request, Func<Dictionary<string, object>, bool> cb);

        public void SendRequest(string serviceName, Dictionary<string, object> request, Func<Dictionary<string, object>, bool> cb)
        {
            this.Hub.SendRequest(serviceName, request, cb);
        }

        public Dictionary<string, object> AwaitSendRequest(string serviceName, Dictionary<string, object> request)
        {
            List<Dictionary<string, object>> responsePtr = new List<Dictionary<string, object>>();

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
        }

        internal static string SerializeWireData(Dictionary<string, object> data)
        {
            List<string> wireData = new List<string>();
            foreach (string key in data.Keys)
            {
                object value = data[key];
                wireData.Add(key);
                if (value is int)
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
                    default: throw new NotImplementedException();
                }
            }
            return output;
        }
    }
}
