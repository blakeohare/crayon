using Common;
using System.Collections.Generic;

namespace Exporter.Workers
{
    public class CreateFileOutputContextWorker : AbstractCrayonWorker
    {
        public override CrayonWorkerResult DoWorkImpl(CrayonWorkerResult[] args)
        {
            // fileOutputContext = CreateFileOutputContext()
            return new CrayonWorkerResult() { Value = new Dictionary<string, FileOutput>() };
        }
    }
}
