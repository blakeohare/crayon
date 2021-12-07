using System;
using System.Collections.Generic;

namespace U3Windows
{
    internal class U3Window
    {
        private System.Windows.Window nativeWindow;
        private Microsoft.Web.WebView2.Wpf.WebView2 webview;
        private Func<Dictionary<string, object>, bool> onDataReceived;

        private static int idAlloc = 1;
        internal int ID { get; private set; }

        public U3Window()
        {
            this.ID = idAlloc++;
        }

        internal void Show(Func<bool> onCloseCallback)
        {
            this.nativeWindow = new System.Windows.Window() { Title = "U3 Window" };
            this.webview = new Microsoft.Web.WebView2.Wpf.WebView2();
            this.nativeWindow.Content = this.webview;
            this.webview.Loaded += async (sender, args) =>
            {
                await this.webview.EnsureCoreWebView2Async();
                this.webview.NavigateToString("<html><body>Hello, World!</body></html>");
            };

            this.nativeWindow.Closed += (sender, args) =>
            {
                onCloseCallback();
            };

            this.nativeWindow.Show();
        }

        public void SendData(object[] data)
        {

        }

        public void RegisterListener(Func<Dictionary<string, object>, bool> callback)
        {
            this.onDataReceived = callback;
        }

        internal void RequestClose()
        {
            this.nativeWindow.Close();
        }
    }
}
