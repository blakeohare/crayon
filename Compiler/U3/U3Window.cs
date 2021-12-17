using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace U3
{
    internal abstract class U3Window
    {
        internal int ID { get; set; }

        internal Func<Dictionary<string, object>, bool> EventsListener { get; set; }
        internal Func<Dictionary<string, object>, bool> BatchListener { get; set; }
        internal Func<Dictionary<string, object>, bool> ClosedListener { get; set; }
        internal Func<Dictionary<string, object>, bool> LoadedListener { get; set; }

        internal abstract Task<string> CreateAndShowWindow(string title, string icon, int width, int height, bool keepAspectRatio, object[] initialData);

        internal abstract Task SendDataBuffer(object[] buffer);
    }
}
