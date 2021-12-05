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
            int windowId = request.ContainsKey("windowId") ? (int)request["windowId"] : 0;
            string type = (string)request["type"];
            string jsonPayload = request.ContainsKey("jsonPayload") ? (string)request["jsonPayload"] : "{}";
            U3Window window = null;
            if (!windows.ContainsKey(windowId))
            {
                if (type == "create")
                {
                    window = new U3Window(); // doesn't actually show a window, just allocates an object capable of receiving further U3 service messages.
                    windows[window.ID] = window;
                    cb(new Dictionary<string, object>() { { "windowId", window.ID } });
                    return;
                }

                throw new NotImplementedException(); // message sent to non-existent window
            }

            windows[windowId].HandleU3Message(request, type, jsonPayload, cb);
        }
    }
}
