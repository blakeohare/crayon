using System;
using Common;

namespace Crayon
{
    internal class UsageDisplayWorker : AbstractCrayonWorker
    {
        private static readonly string USAGE = Util.JoinLines(
            "Usage:",
            "  crayon BUILD-FILE -target BUILD-TARGET-NAME [OPTIONS...]",
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
            "");

        public override CrayonWorkerResult DoWorkImpl(CrayonWorkerResult[] args)
        {
            Console.WriteLine(USAGE);
            return new CrayonWorkerResult();
        }
    }
}
