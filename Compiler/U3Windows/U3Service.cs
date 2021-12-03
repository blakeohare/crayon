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
            string message = (request["msg"] ?? "").ToString();
            new U3Window(1).Show(message);
            cb(new Dictionary<string, object>());
        }
    }
}
