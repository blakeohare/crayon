namespace Interpreter.Libraries.Nori
{
    [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
    [System.Runtime.InteropServices.ComVisible(true)]
    public class JsBridge
    {
        private NoriFrame owner;
        private System.Windows.Forms.WebBrowser browserControl;

        public JsBridge(NoriFrame owner, System.Windows.Forms.WebBrowser browserControl)
        {
            this.owner = owner;
            this.browserControl = browserControl;
        }

        public void SendEventToCSharp(int elementId, string eventName, string value)
        {
            NoriHelper.QueueEventMessage(this.owner, elementId, eventName, value);
        }
    }
}
