using System.Collections.Generic;

namespace Interpreter.Structs
{
    public class NamedCallbackStore
    {
        public List<System.Func<object[], object>> callbacksById;
        public Dictionary<string, Dictionary<string, int>> callbackIdLookup;

        public NamedCallbackStore(List<System.Func<object[], object>> callbacksById, Dictionary<string, Dictionary<string, int>> callbackIdLookup)
        {
            this.callbacksById = callbacksById;
            this.callbackIdLookup = callbackIdLookup;
        }
    }

}
