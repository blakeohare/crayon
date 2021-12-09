using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wax;

namespace U3Windows
{
    public class U3Service : WaxService
    {
        public U3Service() : base("u3") { }

        private Dictionary<int, U3Window> windows = new Dictionary<int, U3Window>();

        public override Task<Dictionary<string, object>> HandleRequest(Dictionary<string, object> request)
        {
            U3Window window = null;
            this.windows.TryGetValue(GetValue<int>(request, "windowId", 0), out window);

            switch (GetValue(request, "type", ""))
            {
                case "prepareWindow":
                    window = new U3Window();
                    this.windows[window.ID] = window;
                    return Task.FromResult(new Dictionary<string, object>() { { "windowId", window.ID } });

                case "show":
                    {
                        int width = GetValue<int>(request, "width", 0);
                        int height = GetValue<int>(request, "height", 0);
                        string icon = GetValue<string>(request, "icon", "");
                        object[] initialData = GetValue<object[]>(request, "initialData", null) ?? new object[0];
                        string title = GetValue<string>(request, "title", "U3 Window");
                        bool keepAspectRatio = GetValue<bool>(request, "keepAspectRatio", false);

                        TaskCompletionSource<Dictionary<string, object>> windowShownTask = new TaskCompletionSource<Dictionary<string, object>>();
                        window.Show(title, width, height, icon, keepAspectRatio, initialData, () =>
                        {
                            windowShownTask.SetResult(new Dictionary<string, object>());
                            return true;
                        });
                        return windowShownTask.Task;
                    }

                default:
                    throw new NotImplementedException();
            }
        }

        public override void RegisterListener(Dictionary<string, object> request, Func<Dictionary<string, object>, bool> callback)
        {
            U3Window window = null;
            this.windows.TryGetValue(GetValue<int>(request, "windowId", 0), out window);

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

        private static T GetValue<T>(Dictionary<string, object> request, string key, T defaultValue)
        {
            object value;
            if (request.TryGetValue(key, out value) && value is T) return (T)value;
            return defaultValue;
        }
    }
}
