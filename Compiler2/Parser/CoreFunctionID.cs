using Parser.ParseTree;

namespace Parser
{
    internal enum CoreFunctionID
    {
        PARSE_INT = 1,
        PARSE_FLOAT = 2,
        PRINT = 3,
        TYPE_OF = 4,
        TYPE_IS = 5,
        EXECUTION_CONTEXT_ID = 6,
        ASSERT = 7,
        CHR = 8,
        ORD = 9,
        CURRENT_TIME = 10,
        SORT_LIST = 11,
        ABS = 12,
        ARC_COS = 13,
        ARC_SIN = 14,
        ARC_TAN = 15,
        COS = 16,
        ENSURE_RANGE = 17,
        FLOOR = 18,
        MAX = 19,
        MIN = 20,
        NATIVE_INT = 21,
        NATIVE_STRING = 22,
        SIGN = 23,
        SIN = 24,
        TAN = 25,
        LN = 26,
        INT_QUEUE_CLEAR = 27,
        INT_QUEUE_WRITE_16 = 28,
        EXECUTION_CONTEXT_COUNTER = 29,
        SLEEP = 30,
        PROJECT_ID = 31,
        IS_JAVASCRIPT = 32,
        IS_ANDROID = 33,
        ALLOC_NATIVE_DATA = 34,
        SET_NATIVE_DATA = 35,
        GET_EXCEPTION_TRACE = 36,
        REFLECT_ALL_CLASSES = 37,
        REFLECT_GET_METHODS = 38,
        REFLECT_GET_CLASS = 39,
        CONVERT_LOCAL_FLOATS_TO_INTS = 40,
        ADD_SHUTDOWN_HANDLER = 41,
        NATIVE_TUNNEL_SEND = 42,
        NATIVE_TUNNEL_RECV = 43,
    }

    internal static class CoreFunctionIDHelper
    {
        public static int GetId(string value)
        {
            switch (value)
            {
                case "parseInt": return (int)CoreFunctionID.PARSE_INT;
                case "parseFloat": return (int)CoreFunctionID.PARSE_FLOAT;
                case "print": return (int)CoreFunctionID.PRINT;
                case "typeof": return (int)CoreFunctionID.TYPE_OF;
                case "typeis": return (int)CoreFunctionID.TYPE_IS;
                case "execCounter": return (int)CoreFunctionID.EXECUTION_CONTEXT_COUNTER;
                case "execId": return (int)CoreFunctionID.EXECUTION_CONTEXT_ID;
                case "assert": return (int)CoreFunctionID.ASSERT;
                case "chr": return (int)CoreFunctionID.CHR;
                case "ord": return (int)CoreFunctionID.ORD;
                case "currentTime": return (int)CoreFunctionID.CURRENT_TIME;
                case "sortList": return (int)CoreFunctionID.SORT_LIST;
                case "abs": return (int)CoreFunctionID.ABS;
                case "arcCos": return (int)CoreFunctionID.ARC_COS;
                case "arcSin": return (int)CoreFunctionID.ARC_SIN;
                case "arcTan": return (int)CoreFunctionID.ARC_TAN;
                case "cos": return (int)CoreFunctionID.COS;
                case "ensureRange": return (int)CoreFunctionID.ENSURE_RANGE;
                case "floor": return (int)CoreFunctionID.FLOOR;
                case "max": return (int)CoreFunctionID.MAX;
                case "min": return (int)CoreFunctionID.MIN;
                case "nativeInt": return (int)CoreFunctionID.NATIVE_INT;
                case "nativeString": return (int)CoreFunctionID.NATIVE_STRING;
                case "sign": return (int)CoreFunctionID.SIGN;
                case "sin": return (int)CoreFunctionID.SIN;
                case "tan": return (int)CoreFunctionID.TAN;
                case "ln": return (int)CoreFunctionID.LN;
                case "intQueueWrite16": return (int)CoreFunctionID.INT_QUEUE_WRITE_16;
                case "intQueueClear": return (int)CoreFunctionID.INT_QUEUE_CLEAR;
                case "sleep": return (int)CoreFunctionID.SLEEP;
                case "projectId": return (int)CoreFunctionID.PROJECT_ID;
                case "isJavaScript": return (int)CoreFunctionID.IS_JAVASCRIPT;
                case "isAndroid": return (int)CoreFunctionID.IS_ANDROID;
                case "allocNativeData": return (int)CoreFunctionID.ALLOC_NATIVE_DATA;
                case "setNativeData": return (int)CoreFunctionID.SET_NATIVE_DATA;
                case "getExceptionTrace": return (int)CoreFunctionID.GET_EXCEPTION_TRACE;
                case "reflectAllClasses": return (int)CoreFunctionID.REFLECT_ALL_CLASSES;
                case "reflectGetMethods": return (int)CoreFunctionID.REFLECT_GET_METHODS;
                case "reflectGetClass": return (int)CoreFunctionID.REFLECT_GET_CLASS;
                case "convertFloatArgsToInts": return (int)CoreFunctionID.CONVERT_LOCAL_FLOATS_TO_INTS;
                case "addShutdownHandler": return (int)CoreFunctionID.ADD_SHUTDOWN_HANDLER;
                case "nativeTunnelSend": return (int)CoreFunctionID.NATIVE_TUNNEL_SEND;
                case "nativeTunnelRecv": return (int)CoreFunctionID.NATIVE_TUNNEL_RECV;
                default: return -1;
            }
        }

        internal static int GetId(StringConstant str, Localization.Locale locale)
        {
            int output = GetId(str.Value);
            if (output == -1)
            {
                throw ParserException.ThrowException(
                    locale,
                    Localization.ErrorMessages.UNKNOWN_CORE_FUNCTION_ID,
                    str.FirstToken,
                    str.Value);
            }
            return output;
        }
    }
}
