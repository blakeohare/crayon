using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Libraries.Nori
{
    public class NoriFrame
    {
        private static readonly System.Reflection.Assembly thisAssembly = typeof(NoriFrame).Assembly;

        private string title;
        private int width;
        private int height;
        private string initialUiData;

        private System.Windows.Forms.Form form;

        private object browserMutex = new object();
        private System.Windows.Forms.WebBrowser browser;

        public int ResumeExecId { get; private set; }

        public Interpreter.Structs.Value CrayonObjectRef { get; private set; }

        public static NoriFrame CreateAndShow(
            Interpreter.Structs.Value crayonObj,
            string title,
            int width,
            int height,
            string uiData,
            int resumeExecId)
        {
            NoriFrame frame = new NoriFrame(title, width, height, uiData);
            frame.CrayonObjectRef = crayonObj;
            frame.ResumeExecId = resumeExecId;
            frame.Show();
            return frame;
        }

        private NoriFrame(string title, int width, int height, string uiData)
        {
            this.title = title;
            this.width = width;
            this.height = height;
            this.initialUiData = uiData;
        }

        private delegate void BrowserInvoker(string data);
        private delegate void ImageDataInvoker(int id, int width, int height, string dataUri);
        private BrowserInvoker browserInvoker;
        private ImageDataInvoker imageDataInvoker;

        public void SendUiData(string value)
        {
            lock (this.browserMutex)
            {
                if (this.browserInvoker == null)
                {
                    this.browserInvoker = new BrowserInvoker((string data) =>
                    {
                        this.browser.Document.InvokeScript("winFormsNoriHandleNewUiData", new object[] { data });
                    });
                }
                this.browser.Invoke(this.browserInvoker, value);
            }
        }

        public void SendImageToBrowser(int id, int width, int height, string dataUri)
        {
            lock (this.browserMutex)
            {
                if (this.imageDataInvoker == null)
                {
                    this.imageDataInvoker = new ImageDataInvoker((int _id, int _width, int _height, string _data) =>
                    {
                        this.browser.Document.InvokeScript("winFormsPrepImageData", new object[] { _id, _width, _height, _data });
                    });
                }
                this.browser.Invoke(this.imageDataInvoker, new object[] { id, width, height, dataUri });
            }
        }

        private void BuildUi()
        {
            lock (this.browserMutex)
            {
                this.form = new System.Windows.Forms.Form();
                this.browser = new System.Windows.Forms.WebBrowser();
                this.browser.Dock = System.Windows.Forms.DockStyle.Fill;
                this.form.Controls.Add(this.browser);
                this.form.Width = this.width;
                this.form.Height = this.height;
                this.form.Text = this.title;
                this.form.FormClosed += (sender, e) => { NoriHelper.QueueCloseWindowNotification(this); };

                this.browser.ObjectForScripting = new JsBridge(this, this.browser);

                this.browser.AllowNavigation = false;
                this.browser.AllowWebBrowserDrop = false;
                this.browser.IsWebBrowserContextMenuEnabled = false;
                this.browser.ScrollBarsEnabled = false;

                this.form.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;

                this.browser.DocumentText = this.GetHtmlDocument(this.initialUiData);
            }
        }

        private void Show()
        {
            System.Threading.ThreadStart ts = new System.Threading.ThreadStart(() =>
            {
                this.BuildUi();
                this.form.ShowDialog();
            });
            System.Threading.Thread thread = new System.Threading.Thread(ts);
            thread.SetApartmentState(System.Threading.ApartmentState.STA);
            thread.Start();
        }

        internal void Close()
        {
            this.form.Close();
        }

        private string GetHtmlDocument(string initialUiData)
        {
            Dictionary<string, string> codeFiles = ReadNoriCodeFiles();
            string indexHtml = codeFiles["index.html"];

            return indexHtml
                .Replace("%%%JAVASCRIPT_SHIM%%%", codeFiles["shim.js"])
                .Replace("%%%JAVASCRIPT_MAIN%%%", codeFiles["nori.js"])
                .Replace("%%%JAVASCRIPT_INIT_DATA%%%", initialUiData);
        }

        private static Dictionary<string, string> ReadNoriCodeFiles()
        {
            Dictionary<string, string> output = new Dictionary<string, string>();
            foreach (string filename in thisAssembly.GetManifestResourceNames())
            {
                string marker = ".Nori.TextResources.";
                int markerLoc = filename.IndexOf(marker);

                if (markerLoc != -1 && (filename.EndsWith(".js") || filename.EndsWith(".html")))
                {
                    string logicalName = filename.Substring(markerLoc + marker.Length);
                    logicalName = logicalName.Replace('.', '/');
                    int dot = logicalName.LastIndexOf('/');
                    logicalName = logicalName.Substring(0, dot) + "." + logicalName.Substring(dot + 1);
                    output[logicalName] = LoadResourceText(filename);
                }
            }
            return output;
        }

        private static string LoadResourceText(string path)
        {
            byte[] fileContents = LoadResourceBytes(path);
            string maybeBom = string.Join(",", fileContents.Take(3).Select(b => (int)b));
            switch (maybeBom)
            {
                case "239,187,191":
                    fileContents = fileContents.Skip(3).ToArray();
                    break;
            }
            return System.Text.Encoding.UTF8.GetString(fileContents);
        }

        private static readonly byte[] buffer = new byte[500];

        private static byte[] LoadResourceBytes(string path)
        {
            System.IO.Stream stream = thisAssembly.GetManifestResourceStream(path);
            List<byte> output = new List<byte>();
            int bytesRead = 1;
            do
            {
                bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    if (bytesRead == buffer.Length)
                    {
                        output.AddRange(buffer);
                    }
                    else
                    {
                        for (int i = 0; i < bytesRead; ++i)
                        {
                            output.Add(buffer[i]);
                        }
                    }
                }
            } while (bytesRead > 0);
            return output.ToArray();
        }
    }
}
