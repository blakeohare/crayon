using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace U3Windows
{
    public class U3Service : Wax.WaxService
    {
        public U3Service() : base("u3") { }

        public override Task<Dictionary<string, object>> HandleRequest(Dictionary<string, object> request)
        {
            throw new NotImplementedException();
        }
    }
}
