using Common;
using System.Collections.Generic;

namespace Exporter.Workers
{
    public class EmitFilesToDiskWorker : AbstractCrayonWorker
    {
        public override CrayonWorkerResult DoWorkImpl(CrayonWorkerResult[] args)
        {
            // EmitFilesToDisk(fileOutputContext, outputDirectory)
            string fullyQualifiedOutputFolder = (string)args[0].Value;
            Dictionary<string, FileOutput> fileOutputDescriptor = (Dictionary<string, FileOutput>)args[1].Value;
            FileUtil.EnsureParentFolderExists(fullyQualifiedOutputFolder);
            new FileOutputExporter(fullyQualifiedOutputFolder).ExportFiles(fileOutputDescriptor);
            return new CrayonWorkerResult();
        }
    }
}
