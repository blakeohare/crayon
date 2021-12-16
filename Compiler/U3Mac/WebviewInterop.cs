using System;
using System.Runtime.InteropServices;

namespace U3
{
    internal class WebviewInterop
    {
        private IntPtr db = IntPtr.Zero;
        private object dbMtx = new object();

        public WebviewInterop()
        {
            this.db = initialize_window_db();
        }

        [DllImport("u3native.dylib")]
        private static extern IntPtr initialize_window_db();
        [DllImport("u3native.dylib")]
        private static extern IntPtr create_window(IntPtr db);
        [DllImport("u3native.dylib")]
        private static extern int set_window_title(IntPtr win, string title);
        [DllImport("u3native.dylib")]
        private static extern int set_window_size(IntPtr win, int width, int height);
        [DllImport("u3native.dylib")]
        private static extern int set_window_uri(IntPtr win, string uri);
        [DllImport("u3native.dylib")] 
        private static extern int run_window_till_closed(IntPtr win);
        [DllImport("u3native.dylib")]
        private static extern void release_window(IntPtr db, IntPtr win);

        internal IntPtr CreateWindow() {
            lock(this.dbMtx) {
                return create_window(this.db);
            }
        }

        internal void SetTitle(IntPtr window, string title) {
            set_window_title(window, title);
        }

        internal void SetSize(IntPtr window, int width, int height) {
          set_window_size(window, width, height);
        }
        internal void SetContent(IntPtr window, string html) {
            byte[] htmlUtf8 = System.Text.Encoding.UTF8.GetBytes(html);
            string dataUri = "data:text/html;base64," + Convert.ToBase64String(htmlUtf8);
            set_window_uri(window, dataUri);
        }
        internal void RunWindowBlocking(IntPtr window) {
            run_window_till_closed(window);
        }
        internal void ReleaseWindow(IntPtr window) {
          lock(this.dbMtx) {
            release_window(this.db, window);
          }
        }
    }
}