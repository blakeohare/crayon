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
        IPC_NAMED_PIPE_CREATE = 45,
        IPC_NAMED_PIPE_SEND = 46,
        IPC_NAMED_PIPE_FLUSH = 47,
        RANDOM_FLOAT = 48,
        RANDOM_INT = 49,
        RESOURCE_GET_MANIFEST = 50,
        RESOURCE_GET_TEXT = 51,
        ENVIRONMENT_GET_VARIABLE = 52,
        SRANDOM_POPULATE_QUEUE = 53,
        DATETIME_GET_UTC_OFFSET_AT = 54,
        DATETIME_INIT_TIME_ZONE = 55,
        DATETIME_INIT_TIME_ZONE_LIST = 56,
        DATETIME_IS_DST_OCCURRING_AT = 57,
        DATETIME_PARSE_DATE = 58,
        DATETIME_UNIX_TO_STRUCTURED = 59,
        IPC_NAMED_PIPE_SERVER_CREATE = 60,
        IPC_NAMED_PIPE_SERVER_CLOSE = 61,
        JS_INTEROP_INVOKE = 63,
        JS_INTEROP_REGISTER_CALLBACK = 64,
        JS_INTEROP_CALLBACK_RETURN = 65,
        IMAGE_CREATE = 66,
        IMAGE_GET_PIXEL = 67,
        IMAGE_SET_PIXEL = 68,
        IMAGE_SCALE = 69,
        IMAGE_SESSION_START = 70,
        IMAGE_SESSION_FINISH = 71,
        IMAGE_BLIT = 72,
        IMAGE_ATLAS_MANIFEST = 73,
        IMAGE_LOAD_CHUNK = 74,
        IMAGE_GET_CHUNK_SYNC = 75,
        MAKE_BYTE_LIST = 76,
        BYTES_OBJ_TO_LIST = 77,
        HTTP_SEND = 78,
        IMAGE_FROM_BYTES = 79,
        IMAGE_B64_BYTES_PREFERRED = 80,
        IMAGE_ENCODE = 81,
        ARG_VERIFY_INT_RANGE = 82,
        ARG_VERIFY_NUMS = 83,
        XML_PARSE = 84,
        LAUNCH_BROWSER = 85,
        CRYPTO_DIGEST= 86,
        BYTES_TO_TEXT = 87,
        TEXT_TO_BYTES = 88,
        JSON_SERIALIZE = 89,
        ENVIRONMENT_DESCRIPTOR = 90,
        JSON_PARSE = 91,
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
                case "ipcNamedPipeCreate": return (int)CoreFunctionID.IPC_NAMED_PIPE_CREATE;
                case "ipcNamedPipeSend": return (int)CoreFunctionID.IPC_NAMED_PIPE_SEND;
                case "ipcNamedPipeFlush": return (int)CoreFunctionID.IPC_NAMED_PIPE_FLUSH;
                case "randomFloat": return (int)CoreFunctionID.RANDOM_FLOAT;
                case "randomInt": return (int)CoreFunctionID.RANDOM_INT;
                case "resourceGetManifest": return (int)CoreFunctionID.RESOURCE_GET_MANIFEST;
                case "resourceGetText": return (int)CoreFunctionID.RESOURCE_GET_TEXT;
                case "environmentGetVariable": return (int)CoreFunctionID.ENVIRONMENT_GET_VARIABLE;
                case "srandomPopulateQueue": return (int)CoreFunctionID.SRANDOM_POPULATE_QUEUE;
                case "dateTimeGetUtcOffsetAt": return (int)CoreFunctionID.DATETIME_GET_UTC_OFFSET_AT;
                case "dateTimeInitTimeZone": return (int)CoreFunctionID.DATETIME_INIT_TIME_ZONE;
                case "dateTimeInitTimeZoneList": return (int)CoreFunctionID.DATETIME_INIT_TIME_ZONE_LIST;
                case "dateTimeIsDstOccurringAt": return (int)CoreFunctionID.DATETIME_IS_DST_OCCURRING_AT;
                case "dateTimeParseDate": return (int)CoreFunctionID.DATETIME_PARSE_DATE;
                case "dateTimeUnixToStructured": return (int)CoreFunctionID.DATETIME_UNIX_TO_STRUCTURED;
                case "ipcNamedPipeServerCreate": return (int)CoreFunctionID.IPC_NAMED_PIPE_SERVER_CREATE;
                case "ipcNamedPipeServerClose": return (int)CoreFunctionID.IPC_NAMED_PIPE_SERVER_CLOSE;
                case "jsInteropInvoke": return (int)CoreFunctionID.JS_INTEROP_INVOKE;
                case "jsInteropRegisterCallback": return (int)CoreFunctionID.JS_INTEROP_REGISTER_CALLBACK;
                case "jsInteropCallbackReturn": return (int)CoreFunctionID.JS_INTEROP_CALLBACK_RETURN;
                case "imageCreate": return (int)CoreFunctionID.IMAGE_CREATE;
                case "imageGetPixel": return (int)CoreFunctionID.IMAGE_GET_PIXEL;
                case "imageSetPixel": return (int)CoreFunctionID.IMAGE_SET_PIXEL;
                case "imageScale": return (int)CoreFunctionID.IMAGE_SCALE;
                case "imageSessionStart": return (int)CoreFunctionID.IMAGE_SESSION_START;
                case "imageSessionFinish": return (int)CoreFunctionID.IMAGE_SESSION_FINISH;
                case "imageBlit": return (int)CoreFunctionID.IMAGE_BLIT;
                case "imageAtlasManifest": return (int)CoreFunctionID.IMAGE_ATLAS_MANIFEST;
                case "imageLoadChunk": return (int)CoreFunctionID.IMAGE_LOAD_CHUNK;
                case "imageGetChunkSync": return (int)CoreFunctionID.IMAGE_GET_CHUNK_SYNC;
                case "makeByteList": return (int)CoreFunctionID.MAKE_BYTE_LIST;
                case "bytesObjToList": return (int)CoreFunctionID.BYTES_OBJ_TO_LIST;
                case "httpSend": return (int)CoreFunctionID.HTTP_SEND;
                case "imageFromBytes": return (int)CoreFunctionID.IMAGE_FROM_BYTES;
                case "imageB64BytesPreferred": return (int)CoreFunctionID.IMAGE_B64_BYTES_PREFERRED;
                case "imageEncode": return (int)CoreFunctionID.IMAGE_ENCODE;
                case "argVerifyIntRange": return (int)CoreFunctionID.ARG_VERIFY_INT_RANGE;
                case "argVerifyNums": return (int)CoreFunctionID.ARG_VERIFY_NUMS;
                case "xmlParse": return (int)CoreFunctionID.XML_PARSE;
                case "launchBrowser": return (int)CoreFunctionID.LAUNCH_BROWSER;
                case "cryptoDigest": return (int)CoreFunctionID.CRYPTO_DIGEST;
                case "bytesToText": return (int)CoreFunctionID.BYTES_TO_TEXT;
                case "textToBytes": return (int)CoreFunctionID.TEXT_TO_BYTES;
                case "jsonSerialize": return (int)CoreFunctionID.JSON_SERIALIZE;
                case "environmentDescriptor": return (int)CoreFunctionID.ENVIRONMENT_DESCRIPTOR;
                case "jsonParser": return (int)CoreFunctionID.JSON_PARSE;
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
