namespace CommonUtil
{
    public static class Print
    {
        public static void Line(object value)
        {
            System.Console.WriteLine(value);
        }

        public static void ErrorLine(object value)
        {
            System.Console.Error.WriteLine(value);
        }
    }
}
