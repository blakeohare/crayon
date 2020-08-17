using Common;

namespace Crayon
{
    internal class VersionDisplayWorker
    {
        public void DoWorkImpl()
        {
            ConsoleWriter.Print(ConsoleMessageType.USAGE_NOTES, Common.VersionInfo.VersionString);
        }
    }
}
