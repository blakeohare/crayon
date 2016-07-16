namespace Crayon
{
    public enum CoreFunctionID
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
    }

    public static class CoreFunctionIDHelper
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
                default: return -1;
            }
        }

        internal static int GetId(Crayon.ParseTree.StringConstant str)
        {
            int output = GetId(str.Value);
            if (output == -1)
            {
                throw new ParserException(str.FirstToken, "Unknown Core function name: '" + str.Value + "'");
            }
            return output;
        }
    }
}
