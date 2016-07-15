namespace Crayon
{
    public enum CoreFunctionID
    {
        PARSE_INT = 1,
        PARSE_FLOAT = 2,
    }

    public static class CoreFunctionIDHelper
    {
        public static int GetId(string value)
        {
            switch (value)
            {
                case "parseInt": return (int)CoreFunctionID.PARSE_INT;
                case "parseFloat": return (int)CoreFunctionID.PARSE_FLOAT;
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
