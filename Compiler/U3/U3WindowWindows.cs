namespace U3
{
#if WINDOWS
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    internal class U3WindowWindows : U3Window
    {
        private System.Windows.Window nativeWindow;
        private Microsoft.Web.WebView2.Wpf.WebView2 webview;
        private System.Windows.Threading.Dispatcher dispatcher;

        public U3WindowWindows() { }

        internal override Task<string> CreateAndShowWindowImpl(string title, byte[] nullableIcon, int width, int height, Func<string, string, bool> handleVmBoundMessage)
        {
            TaskCompletionSource<string> completionTask = new TaskCompletionSource<string>();
            System.Threading.Thread thread = new System.Threading.Thread(() =>
            {
                this.nativeWindow = new System.Windows.Window() { Title = title, Width = width, Height = height };
                this.webview = new Microsoft.Web.WebView2.Wpf.WebView2();
                this.nativeWindow.Content = this.webview;

                if (nullableIcon != null)
                {
                    this.nativeWindow.Icon = System.Windows.Media.Imaging.BitmapFrame.Create(new System.IO.MemoryStream(nullableIcon));
                }

                this.nativeWindow.Loaded += async (sender, e) =>
                {
                    await this.webview.EnsureCoreWebView2Async();

                    this.dispatcher = this.nativeWindow.Dispatcher;

                    this.webview.CoreWebView2.AddHostObjectToScript("u3bridge", new JsBridge((type, payloadJson) =>
                    {
                        handleVmBoundMessage(type, payloadJson);
                        return "{}";
                    }));

                    LoadHtmlInWebview();
                };

                this.ApplyCloseCauseHandlers();

                this.nativeWindow.ShowDialog();

                completionTask.TrySetResult(this.closeCause);
            });
            thread.SetApartmentState(System.Threading.ApartmentState.STA);
            thread.Start();
            return completionTask.Task;
        }

        internal override Task SendJsonData(string jsonString)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            string serializedString = Wax.Util.JsonUtil.SerializeStringRoot(jsonString);

            this.dispatcher.BeginInvoke((Action)(() =>
            {
                string js = "window.csharpToJavaScript(" + serializedString + ");";
                this.webview.ExecuteScriptAsync(js);
                tcs.SetResult(true);
            }));
            return tcs.Task;
        }

        private string closeCause = "close-button";

        // Detecting whether or not a window was closed due to Alt + F4 or for the close button is not really a supported feature of the framework
        // and so this is a really HACKY way to make that determination. It's not a good way, it's just the only way as far as I can tell.
        private void ApplyCloseCauseHandlers()
        {
            bool altPressed = false;
            bool systemPressed = false; // alt pressed while the window content doesn't have focus causes the key event to be marked as Key.System instead of Key.LeftAlt, etc.

            this.nativeWindow.Closing += (sender, e) =>
            {
                if (systemPressed)
                {
                    // If the window closes while the "system" key (Alt while out of focus) is pressed, assume this is Alt + F4. This is NOT 100% accurate but will
                    // likely catch most cases.
                    this.closeCause = "alt-f4";
                }
            };

            this.nativeWindow.KeyDown += (sender, e) =>
            {
                if (e.Key == System.Windows.Input.Key.F4)
                {
                    if (altPressed)
                    {
                        this.closeCause = "alt-f4";
                    }
                }
                else if (e.Key == System.Windows.Input.Key.LeftAlt || e.Key == System.Windows.Input.Key.RightAlt)
                {
                    altPressed = true;
                }
                else if (e.Key == System.Windows.Input.Key.System)
                {
                    systemPressed = true;
                }
            };

            this.nativeWindow.KeyUp += (sender, e) =>
            {
                if (e.Key == System.Windows.Input.Key.LeftAlt || e.Key == System.Windows.Input.Key.RightAlt)
                {
                    altPressed = false;
                }
                else if (e.Key == System.Windows.Input.Key.System)
                {
                    systemPressed = false;
                }
            };
        }

        private void LoadHtmlInWebview()
        {
#if DEBUG
            // NavigateToString uses a data URI and so the JavaScript source is not available in the developer tools panel
            // when errors occur. Writing to a real file on disk avoids this problem in Debug builds.
            string tempDir = Environment.GetEnvironmentVariable("TEMP");
            string debugHtmlFile = System.IO.Path.Combine(tempDir, "u3-debug.html");
            System.IO.File.WriteAllText(debugHtmlFile, JsResourceUtil.GetU3Source());
            this.webview.CoreWebView2.Navigate("file:///" + debugHtmlFile.Replace('\\', '/'));
#else
            this.webview.NavigateToString(JsResourceUtil.GetU3Source());
#endif
        }
    }
#endif
}
