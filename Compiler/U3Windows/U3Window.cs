﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace U3Windows
{
    internal class U3Window
    {
        private static int idAlloc = 1;

        public int ID { get; private set; }
        public Func<Dictionary<string, object>, bool> EventsListener { get; set; }
        public Func<Dictionary<string, object>, bool> BatchListener { get; set; }
        public Func<Dictionary<string, object>, bool> ClosedListener { get; set; }
        public Func<Dictionary<string, object>, bool> LoadedListener { get; set; }
        public Func<Task> EventLoopTickler { get; set; }

        private System.Windows.Window nativeWindow;
        private Microsoft.Web.WebView2.Wpf.WebView2 webview;

        // Since this is all in the same process, there should be a way to invoke "tickles" from the event loop.
        private System.Windows.Threading.DispatcherTimer eventLoopTicklerTimer;

        public U3Window()
        {
            this.ID = idAlloc++;
        }

        public async Task Show(string title, int width, int height, string icon, bool keepAspectRatio, object[] initialData, int vmId, Func<bool> closedCallback)
        {
            this.nativeWindow = new System.Windows.Window() { Title = title, Width = width, Height = height };
            this.webview = new Microsoft.Web.WebView2.Wpf.WebView2();
            this.nativeWindow.Content = this.webview;

            this.nativeWindow.Loaded += (sender, e) => { LoadedHandler(initialData, keepAspectRatio); };

            await Task.Delay(1);
            this.nativeWindow.ShowDialog();

            this.eventLoopTicklerTimer.Stop();
        }

        private void SendToJavaScript(Dictionary<string, object> data)
        {
            string js = "window.csharpToJavaScript(" + Wax.Util.JsonUtil.SerializeStringRoot(Wax.Util.JsonUtil.SerializeJson(data)) + ");";
            this.webview.ExecuteScriptAsync(js);
        }

        private async void LoadedHandler(object[] initialData, bool keepAspectRatio)
        {
            this.eventLoopTicklerTimer = new System.Windows.Threading.DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(1) };
            this.eventLoopTicklerTimer.Tick += (sender, e) =>
            {
                this.EventLoopTickler();
            };

            await this.webview.EnsureCoreWebView2Async();

            List<Dictionary<string, object>> queueEventsMessages = new List<Dictionary<string, object>>();

            this.webview.CoreWebView2.AddHostObjectToScript("u3bridge", new JsBridge((type, payloadJson) =>
            {
                IDictionary<string, object> wrappedData = new Wax.Util.JsonParser("{\"rawData\": " + payloadJson + "}").ParseAsDictionary();
                object rawData = wrappedData["rawData"];
                int bridgeThreadId1 = System.Threading.Thread.CurrentThread.ManagedThreadId;
                switch (type)
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
                        this.eventLoopTicklerTimer.Start();
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

                    default:
                        throw new NotImplementedException();
                }
                return "{}";
            }));

#if DEBUG
            string tempDir = System.Environment.GetEnvironmentVariable("TEMP");
            string debugHtmlFile = System.IO.Path.Combine(tempDir, "u3-debug.html");
            System.IO.File.WriteAllText(debugHtmlFile, GetU3Source());
            this.webview.CoreWebView2.Navigate("file:///" + debugHtmlFile.Replace('\\', '/'));
#else
            this.webview.NavigateToString(GetU3Source());
#endif
        }

        private static Wax.EmbeddedResourceReader resourceReader = new Wax.EmbeddedResourceReader(typeof(U3Window).Assembly);

        private static string GetU3Source()
        {
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

            string u3Source = string.Join("\n", newLines);
            return u3Source;
        }
    }
}
