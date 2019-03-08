using Interpreter.Structs;
using System.Collections.Generic;

namespace Interpreter.Libraries.Dispatcher
{
    internal static class DispatcherHelper
    {
        public static void FlushNativeQueue(object[] nativeData, List<Value> output)
        {
            if (nativeData != null)
            {
                lock (nativeData[0])
                {
                    List<Value> accumulated = nativeData[1] as List<Value>;
                    if (accumulated != null)
                    {
                        output.AddRange(accumulated);
                        accumulated.Clear();
                    }
                }
            }
        }
    }
}
