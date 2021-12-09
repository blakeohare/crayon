using System;
using System.Collections.Generic;

namespace U3Windows
{
    internal class U3Window
    {
        private static int idAlloc = 1;

        public int ID { get; private set; }
        public Func<Dictionary<string, object>, bool> EventsListener { get; set; }
        public Func<Dictionary<string, object>, bool> BatchListener { get; set; }
        public Func<Dictionary<string, object>, bool> ClosedListener { get; set; }
        public Func<Dictionary<string, object>, bool> LoadedListener { get; set; }

        public U3Window()
        {
            this.ID = idAlloc++;
        }

        public void Show(string title, int width, int height, string icon, bool keepAspectRatio, object[] initialData, Func<bool> shownAndReadyCallback)
        {

        }
    }
}
