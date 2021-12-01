using System;
using System.Collections.Generic;
using System.Text;

namespace Wax.Util
{
    public static class JsonUtil
    {
        public static string SerializeJson(IDictionary<string, object> root)
        {
            return new JsonBasedObject(root).ToJson();
        }
    }
}
