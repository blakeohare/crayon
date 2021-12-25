using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wax;

namespace U3
{
    public class U3Service : WaxService
    {
        public U3Service() : base("u3") { }

        private int idAlloc = 1;
        private Dictionary<int, U3Window> windows = new Dictionary<int, U3Window>();

        public override async Task<Dictionary<string, object>> HandleRequest(Dictionary<string, object> request)
        {
            U3Window window = GetWindow(request);
            switch (GetValue(request, "type", ""))
            {
                case "prepareWindow":
                    window = PlatformUtil.CreateWindow(idAlloc++, this.Hub);
                    this.windows[window.ID] = window;
                    return new Dictionary<string, object>() {
                        { "windowId", window.ID }
                    };

                case "show":
                    {
                        int width = GetValue<int>(request, "width", 0);
                        int height = GetValue<int>(request, "height", 0);
                        string iconBase64 = GetValue<string>(request, "icon", "");
                        object[] initialData = GetValue<object[]>(request, "initialData", null) ?? new object[0];
                        string title = GetValue<string>(request, "title", "Crayon Window");
                        bool keepAspectRatio = GetValue<bool>(request, "keepAspectRatio", false);
                        string closeMethod = await window.CreateAndShowWindow(
                            title,
                            iconBase64 == "" ? null : Convert.FromBase64String(iconBase64),
                            width, height,
                            keepAspectRatio,
                            initialData);
                        return new Dictionary<string, object>() { { "cause", closeMethod } };
                    }

                case "data":
                    await window.SendData(new Dictionary<string, object>() { { "buffer", request["buffer"] } });
                    return new Dictionary<string, object>();

                default:
                    throw new NotImplementedException();
            }
        }

        public override void RegisterListener(Dictionary<string, object> request, Func<Dictionary<string, object>, bool> callback)
        {
            U3Window window = GetWindow(request);
            if (window == null) return;

            switch (GetValue<string>(request, "type", ""))
            {
                case "events":
                    window.EventsListener = callback;
                    break;
                case "batch":
                    window.BatchListener = callback;
                    break;
                case "closed":
                    window.ClosedListener = callback;
                    break;
                case "loaded":
                    window.LoadedListener = callback;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private U3Window GetWindow(Dictionary<string, object> request)
        {
            if (this.windows.TryGetValue(GetValue<int>(request, "windowId", 0), out U3Window window))
            {
                return window;
            }
            return null;
        }

        private static T GetValue<T>(Dictionary<string, object> request, string key, T defaultValue)
        {
            object value;
            if (request.TryGetValue(key, out value) && value is T) return (T)value;
            return defaultValue;
        }
    }
}
