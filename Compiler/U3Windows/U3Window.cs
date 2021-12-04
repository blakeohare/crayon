using System.Collections.Generic;
using System.Linq;

namespace U3Windows
{
    internal class U3Window
    {
        public int ID { get; private set; }
        public U3Window(int id)
        {
            this.ID = id;
        }

        public void Show(string message)
        {
            System.Windows.Window window = new System.Windows.Window();
            Microsoft.Web.WebView2.Wpf.WebView2 webview = new Microsoft.Web.WebView2.Wpf.WebView2();
            window.Content = webview;
            webview.Loaded += async (sender, e) =>
            {
                await webview.EnsureCoreWebView2Async();
                webview.NavigateToString(GetPageSource());
            };
            window.ShowDialog();
        }

        internal void HandleU3Data(string data)
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
