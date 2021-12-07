using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace U3Windows
{
    public class U3Service : Wax.WaxService
    {
        private Dictionary<int, U3Window> windows = new Dictionary<int, U3Window>();

        public U3Service() : base("u3") { }

        public override Task<Dictionary<string, object>> HandleRequest(Dictionary<string, object> request)
        {
            string type = request.ContainsKey("type") ? (string)request["type"] : "";

            U3Window window = this.GetWindowFromRequest(request);

            switch (type)
            {
                case "prepareWindow":
                    window = new U3Window();
                    this.windows[window.ID] = window;
                    return Task.FromResult(new Dictionary<string, object>() {
                        { "windowId", window.ID },
                    });

                case "show": // this request lasts the duration of the lifetime of the window and returns a response once the window is closed.
                    {
                        TaskCompletionSource<Dictionary<string, object>> tcs = new TaskCompletionSource<Dictionary<string, object>>();
                        window.Show(() =>
                        {
                            tcs.SetResult(new Dictionary<string, object>());
                            return true;
                        });
                        return tcs.Task;
                    }

                case "closeRequest":
                    window.RequestClose();
                    return Task.FromResult(new Dictionary<string, object>());

                case "sendData":
                    if (window == null) throw new Exception();
                    window.SendData((object[])request["dataBuffer"]);
                    return Task.FromResult(new Dictionary<string, object>() { });

                default:
                    throw new NotImplementedException();
            }
        }

        public override void RegisterListener(Dictionary<string, object> request, Func<Dictionary<string, object>, bool> callback)
        {
            U3Window window = this.GetWindowFromRequest(request);
            if (window == null) throw new Exception(); // This should not happen.

            window.RegisterListener(callback);
        }

        private U3Window GetWindowFromRequest(Dictionary<string, object> request)
        {
            int windowId = 0;
            if (request.ContainsKey("windowId")) int.TryParse((request["windowId"] ?? "0").ToString(), out windowId);

            U3Window targetWindow = null;
            if (windows.ContainsKey(windowId)) targetWindow = this.windows[windowId];
            return targetWindow;
        }
    }
}
