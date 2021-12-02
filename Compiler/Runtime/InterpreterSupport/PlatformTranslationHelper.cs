namespace Interpreter
{
    public static class PlatformTranslationHelper
    {
        public static void PrintStdErr(string value)
        {
            System.Console.Error.WriteLine(value);
        }

        public static void PrintStdOut(string value)
        {
            System.Console.WriteLine(value);
        }
    }
}
