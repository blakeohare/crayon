using System.Collections.Generic;

namespace U3Windows
{
    internal class U3Window
    {
        private static int idAlloc = 1;

        public int ID { get; private set; }
        public U3Window()
        {
            this.ID = idAlloc++;
        }

        private Dictionary<string, List<System.Func<Dictionary<string, object>, bool>>> listeners = new Dictionary<string, List<System.Func<Dictionary<string, object>, bool>>>();

        internal void HandleU3Message(Dictionary<string, object> request, string type, string jsonPayload, System.Func<Dictionary<string, object>, bool> cb)
        {
            if (type[0] == 'l' && type.StartsWith("listen-"))
            {
                string key = type.Substring("listen-".Length);
                List<System.Func<Dictionary<string, object>, bool>> listenersForEvent;
                if (!listeners.TryGetValue(key, out listenersForEvent))
                {
                    listenersForEvent = new List<System.Func<Dictionary<string, object>, bool>>();
                    listeners[key] = listenersForEvent;
                }
                listenersForEvent.Add(cb);
            }
            else
            {
                switch (type)
                {
                    case "init":
                        this.HandleInit(
                            (string)request["title"],
                            (string)request["icon"],
                            (int)request["width"],
                            (int)request["height"],
                            (bool)request["keepAspectRatio"],
                            (object[])request["initialData"],
                            cb);
                        break;

                    case "data":
                        this.HandleData(jsonPayload);
                        cb(new Dictionary<string, object>());
                        break;
                    default:
                        throw new System.NotImplementedException();
                }
            }
        }

        private void HandleInit(string title, string iconB64, int width, int height, bool keepAspectRatio, object[] initialDataw, System.Func<Dictionary<string, object>, bool> cb)
        {
            System.Windows.Window window = new System.Windows.Window();
            Microsoft.Web.WebView2.Wpf.WebView2 webview = new Microsoft.Web.WebView2.Wpf.WebView2();
            window.Content = webview;

            System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
            timer.Tick += (s1, e1) => {
                cb(new Dictionary<string, object>() {
                    { "u3EventLoopDirective", "U3WINDOW_EVENTLOOP_TICK" },
                });
            };
            webview.Loaded += async (sender, e) =>
            {
                await webview.EnsureCoreWebView2Async();
                webview.NavigateToString(GetPageSource());
                timer.Start();
                cb(new Dictionary<string, object>() {
                    { "u3EventLoopDirective", "U3WINDOW_ADDED" },
                });
            };
            window.Closed += (sender, e) => {
                timer.Stop();
                cb(new Dictionary<string, object>() {
                    { "u3EventLoopDirective", "U3WINDOW_REMOVED" },
                });
            };
            window.Title = title;
            window.Width = width;
            window.Height = height;
            window.ShowDialog();
        }

        private void HandleData(string jsonPayload)
        {
            throw new System.NotImplementedException();
        }

        private void Timer_Tick(object sender, System.EventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private string GetPageSource()
        {
            Wax.Util.ResourceLoader resourceLoader = new Wax.Util.ResourceLoader(typeof(U3Window).Assembly);

            string html = resourceLoader.LoadString("js/index.html", "\n");
            string[] lines = html.Split('\n');
            List<string> newLines = new List<string>();
            foreach (string line in lines)
            {
                if (line.Trim().StartsWith("<script src=\""))
                {
                    string[] parts = line.Split('"');
                    string filename = parts[1];
                    newLines.Add("\n\n// file: " + filename + "\n\n");
                    newLines.Add(resourceLoader.LoadString("js/" + filename, "\n"));
                    newLines.Add("");
                }
                else
                {
                    newLines.Add(line);
                }
            }
            string code = string.Join('\n', newLines);
            return code;
        }
    }
}
