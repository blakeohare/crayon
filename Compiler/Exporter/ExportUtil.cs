using CommonUtil.Disk;
using System.Collections.Generic;
using Wax;

namespace Exporter
{
    internal static class ExportUtil
    {
        internal static void EmitFilesToDisk(
            Dictionary<string, FileOutput> fileOutputDescriptor,
            string fullyQualifiedOutputFolder)
        {
            FileUtil.EnsureParentFolderExists(fullyQualifiedOutputFolder);
            new FileOutputExporter(fullyQualifiedOutputFolder).ExportFiles(fileOutputDescriptor);
        }
    }
}
