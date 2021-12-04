using System;
using System.Collections.Generic;

namespace U3Windows
{
    public class U3Service : Wax.WaxService
    {
        public U3Service() : base("u3") { }

        private Dictionary<int, U3Window> windows = new Dictionary<int, U3Window>();

        public override void HandleRequest(Dictionary<string, object> request, Func<Dictionary<string, object>, bool> cb)
        {
            int windowId = request.ContainsKey("windowId") ? (int)request["windowId"] : 1;
            string payload = (string)request["payload"];
            U3Window window = null;
            if (!windows.ContainsKey(windowId))
            {
                window = new U3Window(windowId);
                windows[windowId] = window;
            }
            window.HandleU3Data(payload);
            cb(new Dictionary<string, object>());
        }
    }
}
