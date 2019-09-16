using Common;

namespace Crayon
{
    internal class UsageDisplayWorker
    {
        private static readonly string USAGE = CommonUtil.Core.StringUtil.JoinLines(
            "Crayon version " + VersionInfo.VersionString,
            "",
            "To export:",
            "  crayon BUILD-FILE -target BUILD-TARGET-NAME [OPTIONS...]",
            "",
            "To run:",
            "  crayon BUILD-FILE [args...]",
            "",
            "Flags:",
            "",
            "  -target            When a build file is specified, selects the",
            "                     target within that build file to build.",
            "",
            "  -vm                Output a standalone VM for a platform.",
            "",
            "  -vmdir             Directory to output the VM to (when -vm is",
            "                     specified).",
            "",
            "  -genDefaultProj    Generate a default boilerplate project to",
            "                     the current directory.",
            "",
            "  -genDefaultProjES  Generates a default project with ES locale.",
            "",
            "  -genDefaultProjJP  Generates a default project with JP locale.",
            "",
            "  -showLibDepTree    Shows a dependency tree of the build.",
            "",
            "  -showLibStack      Stack traces will include libraries. By",
            "                     default, stack traces are truncated to only",
            "                     show user code.",
            "");

        public void DoWorkImpl()
        {
            ConsoleWriter.Print(ConsoleMessageType.USAGE_NOTES, USAGE);
        }
    }
}
