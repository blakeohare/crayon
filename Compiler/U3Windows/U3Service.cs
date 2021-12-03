using System;
using System.Collections.Generic;

namespace U3Windows
{
    public class U3Service : Wax.WaxService
    {
        public U3Service() : base("u3") { }

        private Dictionary<int, U3Session> sessions = new Dictionary<int, U3Session>();

        public override void HandleRequest(Dictionary<string, object> request, Func<Dictionary<string, object>, bool> cb)
        {
            throw new NotImplementedException();
        }
    }
}
