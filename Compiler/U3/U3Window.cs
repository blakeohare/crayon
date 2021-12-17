using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace U3
{
    internal abstract class U3Window
    {
        internal int ID { get; set; }

        internal Func<Dictionary<string, object>, bool> EventsListener { get; set; }
        internal Func<Dictionary<string, object>, bool> BatchListener { get; set; }
        internal Func<Dictionary<string, object>, bool> ClosedListener { get; set; }
        internal Func<Dictionary<string, object>, bool> LoadedListener { get; set; }

        protected bool keepAspectRatio;
        protected object[] initialData;
        protected List<Dictionary<string, object>> queueEventsMessages = new List<Dictionary<string, object>>();

        private void HandleIncomingMessageFromJavaScript(string type, string jsonPayload)
        {
            IDictionary<string, object> wrappedData = new Wax.Util.JsonParser("{\"rawData\": " + jsonPayload + "}").ParseAsDictionary();
            object rawData = wrappedData["rawData"];

            switch (type)
            {
                case "bridgeReady":
                    this.SendData(new Dictionary<string, object>()
                    {
                        { "buffer", initialData },
                        { "options", new Dictionary<string, object>()
                            {
                                { "keepAspectRatio", keepAspectRatio }
                            }
                        }
                    });
                    this.initialData = null;
                    break;

                case "shown":
                    this.LoadedListener(new Dictionary<string, object>());

                    if (queueEventsMessages != null)
                    {
                        foreach (Dictionary<string, object> evMsg in queueEventsMessages)
                        {
                            this.EventsListener(evMsg);
                        }
                        queueEventsMessages = null;
                    }
                    break;

                case "events":
                    Dictionary<string, object> eventData = new Dictionary<string, object>() {
                            { "msgs", rawData },
                        };
                    if (queueEventsMessages != null)
                    {
                        queueEventsMessages.Add(eventData);
                    }
                    else
                    {
                        this.EventsListener(eventData);
                    }
                    break;

                case "eventBatch":
                    this.BatchListener(new Dictionary<string, object>() { { "data", rawData } });
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        internal Task<string> CreateAndShowWindow(string title, byte[] nullableIcon, int width, int height, bool keepAspectRatio, object[] initialData)
        {
            this.keepAspectRatio = keepAspectRatio;
            this.initialData = initialData;
            return this.CreateAndShowWindowImpl(title, nullableIcon, width, height, (type, payload) =>
            {
                this.HandleIncomingMessageFromJavaScript(type, payload);
                return true;
            });
        }

        internal abstract Task<string> CreateAndShowWindowImpl(string title, byte[] nullableIcon, int width, int height, Func<string, string, bool> handleVmBoundMessage);

        internal Task SendData(Dictionary<string, object> data)
        {
            return this.SendJsonData(Wax.Util.JsonUtil.SerializeJson(data));
        }

        internal abstract Task SendJsonData(string jsonString);
    }
}
