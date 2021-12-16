using System;
using System.Collections.Generic;
using System.Linq;

namespace U3
{
    public class U3Window
    {
        [System.Runtime.InteropServices.DllImport("u3native.dylib")]
        internal static extern int show_window(string title, int width, int height, string dataUri);

        private static int idAlloc = 1;
        internal int ID { get; set; }

        public Func<Dictionary<string, object>, bool> EventsListener { get; set; }
        public Func<Dictionary<string, object>, bool> BatchListener { get; set; }
        public Func<Dictionary<string, object>, bool> ClosedListener { get; set; }
        public Func<Dictionary<string, object>, bool> LoadedListener { get; set; }

        private IntPtr nativeWindow;

        private object[] initialData;
        private bool keepAspectRatio;

        public U3Window()
        {
            this.ID = idAlloc++;
        }

        public void Show(string title, int width, int height, bool keepAspectRatio, object[] initialData, System.Threading.Tasks.TaskCompletionSource<string> closeCallback)
        {
            this.initialData = initialData;
            this.keepAspectRatio = keepAspectRatio;

            string html = GetU3Source();
            byte[] htmlUtf8 = System.Text.Encoding.UTF8.GetBytes(html);
            string dataUri = "data:text/html;base64," + Convert.ToBase64String(htmlUtf8);
            int value = show_window(title, width, height, dataUri);
            this.nativeWindow = new IntPtr(value);
        }

        internal void SendDataBuffer(object[] buffer)
        {
            this.SendToJavaScript(new Dictionary<string, object>() { { "buffer", buffer } });
        }

        private void SendToJavaScript(Dictionary<string, object> data)
        {
            string js = "window.csharpToJavaScript(" + Wax.Util.JsonUtil.SerializeStringRoot(Wax.Util.JsonUtil.SerializeJson(data)) + ");";
            throw new System.NotImplementedException("Send to JavaScript not implemented");
        }

        private List<Dictionary<string, object>> queueEventsMessages = new List<Dictionary<string, object>>();


        private void HandleMessageFromJavaScript(string messageType, string payloadJson)
        {
            IDictionary<string, object> wrappedData = new Wax.Util.JsonParser("{\"rawData\": " + payloadJson + "}").ParseAsDictionary();
            object rawData = wrappedData["rawData"];

            switch (messageType)
            {
                case "bridgeReady":
                    this.SendToJavaScript(new Dictionary<string, object>()
                        {
                            { "buffer", initialData },
                            { "options", new Dictionary<string, object>()
                                {
                                    { "keepAspectRatio", keepAspectRatio }
                                }
                            }
                        });
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

        private static string u3Source = null;
        private static string GetU3Source()
        {
            if (u3Source == null)
            {
                Wax.EmbeddedResourceReader resourceReader = new Wax.EmbeddedResourceReader(typeof(U3Window).Assembly);
                string[] lines = resourceReader.GetText("u3/index.html", "\n").Split('\n');
                List<string> newLines = new List<string>();
                foreach (string line in lines)
                {
                    if (line.Contains("SCRIPTS_GO_HERE"))
                    {
                        string[] files = resourceReader.ListFiles("u3/").Where(name => name.EndsWith(".js")).ToArray();

                        foreach (string file in files)
                        {
                            newLines.Add("<script>");
                            newLines.Add("// " + file);
                            newLines.Add(resourceReader.GetText(file, "\n").Trim());
                            newLines.Add("</script>");
                        }
                    }
                    else
                    {
                        newLines.Add(line);
                    }
                }

                u3Source = string.Join("\n", newLines);
            }
            return u3Source;
        }
    }
}
