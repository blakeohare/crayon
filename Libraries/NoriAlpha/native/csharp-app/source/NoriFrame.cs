namespace Interpreter.Libraries.NoriAlpha
{
    public class NoriFrame
    {
        private System.Windows.Forms.Form form;
        private System.Windows.Forms.WebBrowser browser;

        public Interpreter.Structs.Value CrayonObjectRef { get; private set; }

        public static NoriFrame CreateAndShow(Interpreter.Structs.Value crayonObj, string title, int width, int height, string uiData)
        {
            NoriFrame frame = new NoriFrame(title, width, height, uiData);
            frame.CrayonObjectRef = crayonObj;
            frame.Show();
            return frame;
        }

        private NoriFrame(string title, int width, int height, string uiData)
        {
            this.form = new System.Windows.Forms.Form();
            this.browser = new System.Windows.Forms.WebBrowser();
            this.browser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form.Controls.Add(this.browser);
            this.form.Width = width;
            this.form.Height = height;
            this.form.Text = title;
            this.form.FormClosed += (sender, e) => { NoriHelper.QueueCloseWindowNotification(this); };

            this.browser.AllowWebBrowserDrop = false;
            this.browser.ObjectForScripting = new JsBridge(this, this.browser);

            browser.DocumentText = this.GetHtmlDocument(uiData);
        }

        public void SendUiData(string value)
        {
            this.browser.Document.InvokeScript("handleNewUiData", new object[] { value });
        }

        private void Show()
        {
            this.form.ShowDialog();
        }

        internal void Close()
        {
            this.form.Close();
        }

        private string GetHtmlDocument(string initialUiData)
        {
            return string.Join("\n", new string[] {
                "<html>",
                "<body onload=\"init()\">",
                "<script>",
                    "function handleNewUiData(data) {",
                        "var output = document.getElementById('msg');",
                        "output.innerHTML += lines[i] + '<br />';",
                    "}" ,
                    "function pushy() {",
                        "window.external.SendMessageToCSharp(42, 'I pushed the button');",
                    "}",
                    "function init() { ",
                        "var uiData = \"" + initialUiData + "\";",
                        "handleNewUiData(uiData);",
                    "}",
                "</script>",
                "Hello, World!",
                "<div id=\"msg\"></div>",
                "<button onclick=\"pushy()\">Button</button>",
                "</body>",
                "</html>"
            });
        }
    }
}
