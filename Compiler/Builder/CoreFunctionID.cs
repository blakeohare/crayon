using Builder.ParseTree;

namespace Builder
{
    internal enum CoreFunctionID
    {
        // TODO: before the next stable release, rename some of these to clump them together better and then
        // re-number them.
        ABS = 12,
        ALLOC_NATIVE_DATA = 34,
        ARC_COS = 13,
        ARC_SIN = 14,
        ARC_TAN = 15,
        ARG_VERIFY_INT_RANGE = 82,
        ARG_VERIFY_NUMS = 83,
        ASSERT = 7,
        BASE_64_FROM_BYTES = 114,
        BROWSER_INTEROP_GET_URL_PATH = 112,
        BROWSER_INTEROP_SET_URL_PATH = 113,
        BYTES_OBJ_TO_LIST = 77,
        BYTES_TO_TEXT = 87,
        CHR = 8,
        CONVERT_LOCAL_FLOATS_TO_INTS = 40,
        COOKIE_GET = 109,
        COOKIE_SET = 110,
        COS = 16,
        CRYPTO_DIGEST = 86,
        CURRENT_TIME = 10,
        DATETIME_GET_UTC_OFFSET_AT = 54,
        DATETIME_INIT_TIME_ZONE = 55,
        DATETIME_INIT_TIME_ZONE_LIST = 56,
        DATETIME_IS_DST_OCCURRING_AT = 57,
        DATETIME_PARSE_DATE = 58,
        DATETIME_UNIX_TO_STRUCTURED = 59,
        DISK_DIRECTORY_LIST = 103,
        DISK_DIRECTORY_CREATE = 104,
        DISK_DIRECTORY_DELETE = 105,
        DISK_DIRECTORY_MOVE = 106,
        DISK_FILE_INFO = 98,
        DISK_FILE_WRITE = 99,
        DISK_FILE_READ = 100,
        DISK_FILE_DELETE = 101,
        DISK_FILE_MOVE = 102,
        DISK_GET_CURRENT_DIRECTORY = 97,
        DISK_GET_USER_DIRECTORY = 95,
        DISK_INITIALIZE_DISK = 96,
        ENSURE_RANGE = 17,
        ENVIRONMENT_DESCRIPTOR = 90,
        ENVIRONMENT_GET_VARIABLE = 52,
        ENVIRONMENT_IS_MOBILE = 115,
        EXEC_RESUME = 125,
        EXEC_SUSPEND = 126,
        EXECUTION_CONTEXT_ID = 6,
        FLOOR = 18,
        GET_EXCEPTION_TRACE = 36,
        HTTP_SEND = 78,
        IMAGE_ATLAS_MANIFEST = 73,
        IMAGE_B64_BYTES_PREFERRED = 80,
        IMAGE_BLIT = 72,
        IMAGE_CREATE = 66,
        IMAGE_ENCODE = 81,
        IMAGE_FROM_BYTES = 79,
        IMAGE_GET_CHUNK_SYNC = 75,
        IMAGE_GET_PIXEL = 67,
        IMAGE_LOAD_CHUNK = 74,
        IMAGE_SET_PIXEL = 68,
        IMAGE_SCALE = 69,
        IMAGE_SESSION_START = 70,
        IMAGE_SESSION_FINISH = 71,
        IPC_NAMED_PIPE_CREATE = 45,
        IPC_NAMED_PIPE_FLUSH = 47,
        IPC_NAMED_PIPE_SEND = 46,
        IPC_NAMED_PIPE_SERVER_CREATE = 60,
        IPC_NAMED_PIPE_SERVER_CLOSE = 61,
        IPC_UNIX_SOCKET_CLIENT_CREATE = 116,
        IPC_UNIX_SOCKET_CLIENT_SEND = 118,
        IPC_UNIX_SOCKET_SERVER_CREATE = 117,
        IPC_UNIX_SOCKET_SERVER_DISCONNECT = 119,
        JS_INTEROP_CALLBACK_RETURN = 65,
        JS_INTEROP_INVOKE = 63,
        JS_INTEROP_REGISTER_CALLBACK = 64,
        JSON_PARSE = 91,
        JSON_SERIALIZE = 89,
        LAUNCH_BROWSER = 85,
        LN = 26,
        MAKE_BYTE_LIST = 76,
        MAX = 19,
        MIN = 20,
        NATIVE_TUNNEL_SEND = 42,
        NATIVE_TUNNEL_RECV = 43,
        ORD = 9,
        PARSE_INT = 1,
        PARSE_FLOAT = 2,
        PRINT = 3,
        PROCESS_CURRENT_ID = 107,
        PROCESS_KILL = 93,
        PROCESS_LIST = 108,
        PROCESS_RUN = 92,
        PROJECT_ID = 31,
        RANDOM_FLOAT = 48,
        RANDOM_INT = 49,
        REFLECT_ALL_CLASSES = 37,
        REFLECT_GET_CLASS = 39,
        REFLECT_GET_METHODS = 38,
        REFLECT_NAMESPACE_FUNCTIONS = 120,
        RESOURCE_GET_BYTES = 124,
        RESOURCE_GET_MANIFEST = 50,
        RESOURCE_GET_TEXT = 51,
        SET_NATIVE_DATA = 35,
        SIGN = 23,
        SIN = 24,
        SLEEP = 30,
        SORT_LIST = 11,
        SRANDOM_POPULATE_QUEUE = 53,
        TAN = 25,
        TEXT_TO_BYTES = 88,
        TIMED_CALLBACK = 94,
        TYPE_OF = 4,
        TYPE_IS = 5,
        VALUE_TO_FLOAT_OR_NULL = 111,
        WAX_SEND = 121,
        WAX_SERVICE_GET_PAYLOAD = 122,
        WAX_SERVICE_SEND_RESPONSE = 123,
        XML_PARSE = 84,
        // Next ID: 127
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
                case "sign": return (int)CoreFunctionID.SIGN;
                case "sin": return (int)CoreFunctionID.SIN;
                case "tan": return (int)CoreFunctionID.TAN;
                case "ln": return (int)CoreFunctionID.LN;
                case "sleep": return (int)CoreFunctionID.SLEEP;
                case "projectId": return (int)CoreFunctionID.PROJECT_ID;
                case "allocNativeData": return (int)CoreFunctionID.ALLOC_NATIVE_DATA;
                case "setNativeData": return (int)CoreFunctionID.SET_NATIVE_DATA;
                case "getExceptionTrace": return (int)CoreFunctionID.GET_EXCEPTION_TRACE;
                case "reflectAllClasses": return (int)CoreFunctionID.REFLECT_ALL_CLASSES;
                case "reflectGetMethods": return (int)CoreFunctionID.REFLECT_GET_METHODS;
                case "reflectGetClass": return (int)CoreFunctionID.REFLECT_GET_CLASS;
                case "convertFloatArgsToInts": return (int)CoreFunctionID.CONVERT_LOCAL_FLOATS_TO_INTS;
                case "nativeTunnelSend": return (int)CoreFunctionID.NATIVE_TUNNEL_SEND;
                case "nativeTunnelRecv": return (int)CoreFunctionID.NATIVE_TUNNEL_RECV;
                case "ipcNamedPipeCreate": return (int)CoreFunctionID.IPC_NAMED_PIPE_CREATE;
                case "ipcNamedPipeSend": return (int)CoreFunctionID.IPC_NAMED_PIPE_SEND;
                case "ipcNamedPipeFlush": return (int)CoreFunctionID.IPC_NAMED_PIPE_FLUSH;
                case "randomFloat": return (int)CoreFunctionID.RANDOM_FLOAT;
                case "randomInt": return (int)CoreFunctionID.RANDOM_INT;
                case "resourceGetBytes": return (int)CoreFunctionID.RESOURCE_GET_BYTES;
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
                case "jsonParse": return (int)CoreFunctionID.JSON_PARSE;
                case "processRun": return (int)CoreFunctionID.PROCESS_RUN;
                case "processKill": return (int)CoreFunctionID.PROCESS_KILL;
                case "timedCallback": return (int)CoreFunctionID.TIMED_CALLBACK;
                case "diskGetUserDirectory": return (int)CoreFunctionID.DISK_GET_USER_DIRECTORY;
                case "diskInitializeDisk": return (int)CoreFunctionID.DISK_INITIALIZE_DISK;
                case "diskGetCurrentDirectory": return (int)CoreFunctionID.DISK_GET_CURRENT_DIRECTORY;
                case "diskFileInfo": return (int)CoreFunctionID.DISK_FILE_INFO;
                case "diskFileWrite": return (int)CoreFunctionID.DISK_FILE_WRITE;
                case "diskFileRead": return (int)CoreFunctionID.DISK_FILE_READ;
                case "diskFileDelete": return (int)CoreFunctionID.DISK_FILE_DELETE;
                case "diskFileMove": return (int)CoreFunctionID.DISK_FILE_MOVE;
                case "diskDirectoryList": return (int)CoreFunctionID.DISK_DIRECTORY_LIST;
                case "diskDirectoryCreate": return (int)CoreFunctionID.DISK_DIRECTORY_CREATE;
                case "diskDirectoryDelete": return (int)CoreFunctionID.DISK_DIRECTORY_DELETE;
                case "diskDirectoryMove": return (int)CoreFunctionID.DISK_DIRECTORY_MOVE;
                case "execResume": return (int)CoreFunctionID.EXEC_RESUME;
                case "execSuspend": return (int)CoreFunctionID.EXEC_SUSPEND;
                case "processCurrentId": return (int)CoreFunctionID.PROCESS_CURRENT_ID;
                case "processList": return (int)CoreFunctionID.PROCESS_LIST;
                case "cookieGet": return (int)CoreFunctionID.COOKIE_GET;
                case "cookieSet": return (int)CoreFunctionID.COOKIE_SET;
                case "valueToFloatOrNull": return (int)CoreFunctionID.VALUE_TO_FLOAT_OR_NULL;
                case "browserInteropGetUrlPath": return (int)CoreFunctionID.BROWSER_INTEROP_GET_URL_PATH;
                case "browserInteropSetUrlPath": return (int)CoreFunctionID.BROWSER_INTEROP_SET_URL_PATH;
                case "base64FromBytes": return (int)CoreFunctionID.BASE_64_FROM_BYTES;
                case "environmentIsMobile": return (int)CoreFunctionID.ENVIRONMENT_IS_MOBILE;
                case "ipcUnixSocketClientCreate": return (int)CoreFunctionID.IPC_UNIX_SOCKET_CLIENT_CREATE;
                case "ipcUnixSocketServerCreate": return (int)CoreFunctionID.IPC_UNIX_SOCKET_SERVER_CREATE;
                case "ipcUnixSocketClientSend": return (int)CoreFunctionID.IPC_UNIX_SOCKET_CLIENT_SEND;
                case "ipcUnixSocketServerDisconnect": return (int)CoreFunctionID.IPC_UNIX_SOCKET_SERVER_DISCONNECT;
                case "reflectNamespaceFunctions": return (int)CoreFunctionID.REFLECT_NAMESPACE_FUNCTIONS;
                case "waxSend": return (int)CoreFunctionID.WAX_SEND;
                case "waxServiceGetPayload": return (int)CoreFunctionID.WAX_SERVICE_GET_PAYLOAD;
                case "waxServiceSendResponse": return (int)CoreFunctionID.WAX_SERVICE_SEND_RESPONSE;
                default: return -1;
            }
        }

        internal static int GetId(StringConstant str, Builder.Localization.Locale locale)
        {
            int output = GetId(str.Value);
            if (output == -1)
            {
                throw ParserException.ThrowException(
                    locale,
                    Builder.Localization.ErrorMessages.UNKNOWN_CORE_FUNCTION_ID,
                    str.FirstToken,
                    str.Value);
            }
            return output;
        }
    }
}
