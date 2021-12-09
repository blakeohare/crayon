using System;
using System.Collections.Generic;
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

        private System.Windows.Window nativeWindow;
        private Microsoft.Web.WebView2.Wpf.WebView2 webview;

        public U3Window()
        {
            this.ID = idAlloc++;
        }

        public void Show(string title, int width, int height, string icon, bool keepAspectRatio, object[] initialData, Func<bool> closedCallback)
        {
            this.nativeWindow = new System.Windows.Window() { Title = title, Width = width, Height = height };
            this.webview = new Microsoft.Web.WebView2.Wpf.WebView2();
            this.nativeWindow.Content = this.webview;

            this.nativeWindow.Loaded += async (sender, e) =>
            {
                await this.webview.EnsureCoreWebView2Async();
                this.webview.NavigateToString("<html><body style=\"background-color: #808;color:#fff;\">Hello, World</body></html>");
                await Task.Delay(1000);
                this.LoadedListener(new Dictionary<string, object>() { { "shown", true } });
            };

            this.nativeWindow.ShowDialog();
        }

    }
}
