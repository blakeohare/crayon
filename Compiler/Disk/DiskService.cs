using System;
using System.Collections.Generic;

namespace Disk
{
    public class DiskService : CommonUtil.Wax.WaxService
    {
        public DiskService() : base("disk") { }

        public override void HandleRequest(Dictionary<string, object> request, Func<Dictionary<string, object>, bool> cb)
        {
            switch ((string)request["command"])
            {
                case "designateDisk":

                    string realPath = (string)request["realPath"];
                    string intent = (string)request["intent"];

                    throw new NotImplementedException();

            }
        }
    }
}
