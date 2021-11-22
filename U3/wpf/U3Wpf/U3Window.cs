using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace U3Wpf
{
    public class U3WindowHackyInterop : System.Collections.IList
    {
        private U3Window window = new U3Window();
        public U3WindowHackyInterop()
        {
            this.window = new U3Window();
        }

        public object this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool IsFixedSize => throw new NotImplementedException();
        public bool IsReadOnly => throw new NotImplementedException();
        public int Count => throw new NotImplementedException();
        public bool IsSynchronized => throw new NotImplementedException();
        public object SyncRoot => throw new NotImplementedException();
        public void Clear() { throw new NotImplementedException(); }
        public bool Contains(object value) { throw new NotImplementedException(); }
        public void CopyTo(Array array, int index) { throw new NotImplementedException(); }
        public System.Collections.IEnumerator GetEnumerator() { throw new NotImplementedException(); }
        public int IndexOf(object value) { throw new NotImplementedException(); }
        public void Insert(int index, object value) { throw new NotImplementedException(); }
        public void Remove(object value) { throw new NotImplementedException(); }
        public void RemoveAt(int index) { throw new NotImplementedException(); }

        public int Add(object value)
        {
            string[] data = (string[])value;
            
            switch (data[0])
            {
                case "SHOW":
                    this.window.Show();
                    break;
                case "DATA":
                    string dataMessage = data[1];
                    break;
                default:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }
    }

    public class U3Window
    {
        private System.Windows.Window window;
        private Microsoft.Web.WebView2.Wpf.WebView2 webview;

        public U3Window() { }

        public void Show()
        {
            this.window = new System.Windows.Window();
            this.webview = new Microsoft.Web.WebView2.Wpf.WebView2();
            this.window.Content = this.webview;
            this.window.Loaded += async (sender, e) =>
            {
                string scripts = LoadAllJavaScript();
                await this.webview.EnsureCoreWebView2Async();
                this.webview.NavigateToString("<html><head>" + scripts + "\n\n</head><body>Hello, World!</body></html>");
            };
        }

        private static string LoadAllJavaScript()
        {
            System.Reflection.Assembly asm = typeof(U3Window).Assembly;
            string[] names = asm.GetManifestResourceNames();
            StringBuilder sb = new StringBuilder();
            byte[] bytes = new byte[1000];
            List<byte> allBytes = new List<byte>();
            foreach (string name in names.Where(name => name.EndsWith(".js")))
            {
                System.IO.Stream stream = asm.GetManifestResourceStream(name);
                int bytesRead = stream.Read(bytes, 0, bytes.Length);
                if (bytesRead == bytes.Length)
                {
                    allBytes.AddRange(bytes);
                }
                else
                {
                    for (int i = 0; i < bytesRead; i++)
                    {
                        allBytes.Add(bytes[i]);
                    }
                }

                for (int i = 0; i < 2; i++)
                {
                    allBytes.Add((byte)'\n');
                }
            }

            string javascript = System.Text.Encoding.UTF8.GetString(bytes.ToArray());
            return javascript;
        }
    }
}
