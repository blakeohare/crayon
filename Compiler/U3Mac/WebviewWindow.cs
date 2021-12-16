using System;
using System.Threading.Tasks;

namespace U3
{
    internal class WebviewWindow
    {
        private static WebviewInterop interop = new WebviewInterop();
        private IntPtr nativeWindow;

        public WebviewWindow() {
            this.nativeWindow = interop.CreateWindow();
        }

        public void SetTitle(string value) {
            interop.SetTitle(this.nativeWindow, value);
        }

        public void SetSize(int width, int height) {
            interop.SetSize(this.nativeWindow, width, height);
        }

        public void SetContent(string html) {
            interop.SetContent(this.nativeWindow, html);
        }

        public Task<string> Show() {
            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
            System.Threading.Thread thread = new System.Threading.Thread(() => {
                interop.RunWindowBlocking(this.nativeWindow);
                tcs.TrySetResult("close-button");
            });
            thread.SetApartmentState(System.Threading.ApartmentState.STA);
            thread.Start();
            return tcs.Task;
        }
    }
}
