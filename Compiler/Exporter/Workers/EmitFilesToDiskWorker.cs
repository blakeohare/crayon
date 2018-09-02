using Common;
using System.Collections.Generic;

namespace Exporter.Workers
{
    public class EmitFilesToDiskWorker
    {
        public void DoWorkImpl(
            Dictionary<string, FileOutput> fileOutputDescriptor,
            string fullyQualifiedOutputFolder)
        {
            FileUtil.EnsureParentFolderExists(fullyQualifiedOutputFolder);
            new FileOutputExporter(fullyQualifiedOutputFolder).ExportFiles(fileOutputDescriptor);
        }
    }
}
