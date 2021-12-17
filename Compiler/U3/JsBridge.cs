using System;
using System.Runtime.InteropServices;

namespace U3
{
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true)]
    public class JsBridge
    {
        private Func<string, string, string> vmBoundCallback;
        public JsBridge(Func<string, string, string> vmBoundCallback)
        {
            this.vmBoundCallback = vmBoundCallback;
        }

        public string sendToCSharp(string msgType, string msgPayloadJson)
        {
            this.vmBoundCallback(msgType, msgPayloadJson);
            return "{}";
        }
    }
}
