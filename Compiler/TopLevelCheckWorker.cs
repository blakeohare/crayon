using System;
using Common;

namespace Crayon
{
    internal class TopLevelCheckWorker : AbstractCrayonWorker
    {
        public override string Name { get { return "Crayon::TopLevelCheck"; } }

        public override CrayonWorkerResult DoWorkImpl(CrayonWorkerResult[] args)
        {
            string[] commandLineArgs = Program.GetCommandLineArgs();

            ExportCommand command = FlagParser.Parse(commandLineArgs);

            CrayonWorkerResult result = new CrayonWorkerResult()
            {
                Value = command,
            };
            ExecutionType action = command.IdentifyUseCase();
            switch (action)
            {
                case ExecutionType.SHOW_USAGE: result.SetField("IsDisplayUsage", true); break;
                case ExecutionType.GENERATE_DEFAULT_PROJECT: result.SetField("IsGenerateDefaultProject", true); break;
                case ExecutionType.EXPORT_VM_BUNDLE: result.SetField("IsExportCbxVmBundle", true); break;
                case ExecutionType.EXPORT_VM_STANDALONE: result.SetField("IsExportStandaloneVm", true); break;
                case ExecutionType.EXPORT_CBX: result.SetField("IsExportStandaloneCbx", true); break;
                case ExecutionType.RUN_CBX: result.SetField("IsRunCbx", true); break;
                default: throw new Exception();
            }

            result.SetField("ShowPerformance", command.ShowPerformanceMarkers);

            return result;
        }
    }
}
