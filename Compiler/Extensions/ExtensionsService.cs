using System;
using System.Collections.Generic;

namespace Extensions
{
    public class ExtensionsService : CommonUtil.Wax.WaxService
    {
        public ExtensionsService() : base("extensions") { }

        public override void HandleRequest(Dictionary<string, object> request, Func<Dictionary<string, object>, bool> cb)
        {
            throw new NotImplementedException();
        }
    }
}
