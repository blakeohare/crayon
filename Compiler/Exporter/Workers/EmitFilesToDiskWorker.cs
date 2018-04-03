using Common;
using System.Collections.Generic;

namespace Exporter.Workers
{
    public class EmitFilesToDiskWorker : AbstractCrayonWorker
    {
        public override CrayonWorkerResult DoWorkImpl(CrayonWorkerResult[] args)
        {
            // EmitFilesToDisk(fileOutputContext, outputDirectory)
            Dictionary<string, FileOutput> fileOutputDescriptor = (Dictionary<string, FileOutput>)args[0].Value;
            string fullyQualifiedOutputFolder = (string)args[1].Value;
            FileUtil.EnsureParentFolderExists(fullyQualifiedOutputFolder);
            new FileOutputExporter(fullyQualifiedOutputFolder).ExportFiles(fileOutputDescriptor);
            return new CrayonWorkerResult();
        }
    }
}
