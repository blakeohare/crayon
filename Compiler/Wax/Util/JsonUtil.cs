using System.Collections.Generic;

namespace Wax.Util
{
    public static class JsonUtil
    {
        public static string SerializeJson(IDictionary<string, object> root)
        {
            return new JsonBasedObject(root).ToJson();
        }

        public static string SerializeStringRoot(string value)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            JsonBasedObject.ItemToJson(sb, value);
            return sb.ToString();
        }
    }
}
