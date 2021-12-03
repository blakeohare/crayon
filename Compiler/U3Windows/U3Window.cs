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
            webview.Loaded += async (sender, e) => {
                await webview.EnsureCoreWebView2Async();
                webview.NavigateToString("<html><body><h1>U3 on Windows</h1><p>" + message + "</p></body></html>");
            };
            window.ShowDialog();
        }
    }
}
