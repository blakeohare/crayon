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
        public Func<Task> EventLoopTickler { get; set; }

        private System.Windows.Window nativeWindow;
        private Microsoft.Web.WebView2.Wpf.WebView2 webview;

        // Since this is all in the same process, there should be a way to invoke "tickles" from the event loop.
        private System.Windows.Threading.DispatcherTimer eventLoopTicklerTimer;

        public U3Window()
        {
            this.ID = idAlloc++;
        }

        public void Show(string title, int width, int height, string icon, bool keepAspectRatio, object[] initialData, int vmId, Func<bool> closedCallback)
        {
            this.nativeWindow = new System.Windows.Window() { Title = title, Width = width, Height = height };
            this.webview = new Microsoft.Web.WebView2.Wpf.WebView2();
            this.nativeWindow.Content = this.webview;

            this.nativeWindow.Loaded += LoadedHandler;

            this.nativeWindow.ShowDialog();

            this.eventLoopTicklerTimer.Stop();
        }

        private async void LoadedHandler(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.webview.EnsureCoreWebView2Async();
            this.webview.NavigateToString("<html><body style=\"background-color: #808;color:#fff;\">Hello, World</body></html>");

            // TODO: the JavaScript itself needs to send a loaded notification back to C# instead of this 1 second wait.
            await Task.Delay(1000);

            this.LoadedListener(new Dictionary<string, object>() { { "shown", true } }); // queues up the unblock execution lock

            this.eventLoopTicklerTimer = new System.Windows.Threading.DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(1) };
            this.eventLoopTicklerTimer.Tick += (sender, e) =>
            {
                this.EventLoopTickler();
            };
            this.eventLoopTicklerTimer.Start();
        }

    }
}
