using System.Linq;

namespace Common
{
    public enum ConsoleMessageType
    {
        BUILD_WARNING,
        COMPILER_INFORMATION,
        DEFAULT_PROJ_EXPORT_INFO,
        GENERAL_COMPILATION_ERROR,
        LIBRARY_TREE,
        PARSER_ERROR,
        REMOTE_ASSEMBLY_ERROR,
        STATUS_CHANGE,
        USAGE_NOTES,
    }

    public static class ConsoleWriter
    {
        private static bool prefixesEnabled = false;

        private static InstConsoleWriter instance = new InstConsoleWriter();

        public static InstConsoleWriter Print(ConsoleMessageType messageType, string message)
        {
            if (messageType == ConsoleMessageType.STATUS_CHANGE && !prefixesEnabled)
            {
                return instance;
            }

            string prefix = prefixesEnabled ? (messageType.ToString() + ":") : "";
            foreach (string line in message.Split('\n').Select(s => s.TrimEnd()))
            {
                CommonUtil.Print.Line(prefix + line);
            }

            return instance;
        }

        public static void EnablePrefixes()
        {
            prefixesEnabled = true;
        }
    }

    public class InstConsoleWriter
    {
        public InstConsoleWriter Print(ConsoleMessageType messageType, string message)
        {
            return ConsoleWriter.Print(messageType, message);
        }
    }
}
