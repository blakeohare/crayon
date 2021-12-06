using System;
using System.Collections.Generic;
using System.Linq;

namespace Wax
{
    public class WaxHub
    {
        private Dictionary<string, WaxService> services = new Dictionary<string, WaxService>();

        internal List<string> ExtensionDirectories { get; private set; }

        public string SourceRoot { get; set; }
        public bool ErrorsAsExceptions { get; set; }

        public WaxHub()
        {
            this.ExtensionDirectories = new List<string>();
        }

        public void RegisterService(WaxService service)
        {
            service.Hub = this;
            this.services[service.Name] = service;
        }

        public void RegisterExtensionDirectory(string path)
        {
            this.ExtensionDirectories.Add(path);
        }

        private WaxService GetService(string serviceName)
        {
            if (!this.services.ContainsKey(serviceName))
            {
                CbxExtensionService service = new CbxExtensionService(this, serviceName);
                if (!service.IsPresent) return null;

                this.services[serviceName] = service;
            }
            return this.services[serviceName];
        }

        public Dictionary<string, object> AwaitSendRequest(string serviceName, JsonBasedObject request)
        {
            return AwaitSendRequest(serviceName, request.GetRawData());
        }

        public Dictionary<string, object> AwaitSendRequest(
            string serviceName,
            Dictionary<string, object> request)
        {
            List<Dictionary<string, object>> responsePtr = new List<Dictionary<string, object>>();

            Dictionary<string, object> immutableEnsuredCopy = ParseWireData(SerializeWireData(request));
            WaxService service = this.GetService(serviceName) ?? this.GetService(serviceName + "Extension"); // TODO: do not require the Extension suffix

            if (service == null)
            {
                throw new InvalidOperationException("The extension '" + serviceName + "' could not be found or downloaded.");
            }

            service.HandleRequest(
                immutableEnsuredCopy,
                response =>
                {
                    responsePtr.Add(response);
                    return true;
                });

            if (responsePtr.Count == 0) throw new NotImplementedException();
            return responsePtr[0];
        }

        internal static string SerializeWireData(Dictionary<string, object> data)
        {
            List<string> buffer = new List<string>();
            SerializeWireDataImpl(data, buffer);
            return string.Join(',', buffer.Select(item => Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(item))));
        }

        private static void SerializeWireDataImpl(object item, List<string> buffer)
        {
            if (item == null)
            {
                buffer.Add("N");
            }
            else if (item is int)
            {
                buffer.Add("I");
                buffer.Add("" + item);
            }
            else if (item is bool)
            {
                buffer.Add("B");
                buffer.Add(((bool)item) ? "1" : "0");
            }
            else if (item is string)
            {
                buffer.Add("S");
                buffer.Add((string)item);
            }
            else if (item is string[])
            {
                buffer.Add("As");
                string[] items = (string[])item;
                buffer.Add("" + items.Length);
                buffer.AddRange(items);
            }
            else if (item is int[])
            {
                buffer.Add("Ai");
                int[] items = (int[])item;
                buffer.Add("" + items.Length);
                buffer.AddRange(items.Select(x => "" + x));
            }
            else if (item is object[])
            {
                buffer.Add("Ao");
                object[] items = (object[])item;
                buffer.Add("" + items.Length);
                foreach (object obj in items)
                {
                    SerializeWireDataImpl(obj, buffer);
                }
            }
            else if (item is Dictionary<string, object>)
            {
                buffer.Add("D");
                Dictionary<string, object> d = (Dictionary<string, object>)item;
                buffer.Add("" + d.Count);
                foreach (string key in d.Keys.OrderBy(k => k))
                {
                    buffer.Add(key);
                    SerializeWireDataImpl(d[key], buffer);
                }
            }
            else if (item is JsonBasedObject)
            {
                SerializeWireDataImpl(((JsonBasedObject)item).GetRawData(), buffer);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        internal static Dictionary<string, object> ParseWireData(string encodedData)
        {
            Queue<string> buffer = new Queue<string>(encodedData.Split(',').Select(item => System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(item))));
            object output = ParseWireDataImpl(buffer);
            if (output is Dictionary<string, object>)
            {
                return (Dictionary<string, object>)output;
            }
            throw new InvalidOperationException("Wire data must be a dictionary at its root.");
        }

        private static object ParseWireDataImpl(Queue<string> buffer)
        {
            switch (buffer.Dequeue())
            {
                case "N": return null;
                case "B": return buffer.Dequeue() == "1";
                case "I": return int.Parse(buffer.Dequeue());
                case "S": return buffer.Dequeue();
                case "As":
                    {
                        List<string> strArray = new List<string>();
                        int length = int.Parse(buffer.Dequeue());
                        for (int i = 0; i < length; ++i)
                        {
                            strArray.Add(buffer.Dequeue());
                        }
                        return strArray.ToArray();
                    }
                case "Ai":
                    {
                        List<int> intArray = new List<int>();
                        int length = int.Parse(buffer.Dequeue());
                        for (int i = 0; i < length; ++i)
                        {
                            intArray.Add(int.Parse(buffer.Dequeue()));
                        }
                        return intArray.ToArray();
                    }
                case "Ao":
                    {
                        List<object> objArray = new List<object>();
                        int length = int.Parse(buffer.Dequeue());
                        for (int i = 0; i < length; ++i)
                        {
                            objArray.Add(ParseWireDataImpl(buffer));
                        }
                        return objArray.ToArray();
                    }

                case "D":
                    {
                        Dictionary<string, object> dict = new Dictionary<string, object>();
                        int length = int.Parse(buffer.Dequeue());
                        for (int i = 0; i < length; ++i)
                        {
                            string key = buffer.Dequeue();
                            object value = ParseWireDataImpl(buffer);
                            dict[key] = value;
                        }
                        return dict;
                    }

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
