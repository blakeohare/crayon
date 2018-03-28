using System;
using Common;

namespace Crayon
{
    internal class TopLevelCheckWorker : AbstractCrayonWorker
    {
        private enum ExecutionType
        {
            GENERATE_DEFAULT_PROJECT,
            EXPORT_VM_BUNDLE,
            EXPORT_VM_STANDALONE,
            EXPORT_CBX,
            RUN_CBX,
            SHOW_USAGE,
        }

        public override string Name { get { return "Crayon::TopLevelCheck"; } }

        public override CrayonWorkerResult DoWorkImpl(CrayonWorkerResult[] args)
        {
            string[] commandLineArgs = Program.GetCommandLineArgs();

            ExportCommand command = FlagParser.Parse(commandLineArgs);

            // TODO: I don't like these here.
            command.PlatformProvider = new PlatformProvider();
            command.InlineImportCodeLoader = new InlineImportCodeLoader();

            CrayonWorkerResult result = new CrayonWorkerResult()
            {
                Value = command,
            };

            ExecutionType action = this.IdentifyUseCase(command);
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

        private ExecutionType IdentifyUseCase(ExportCommand command)
        {
            if (command.IsGenerateDefaultProject) return ExecutionType.GENERATE_DEFAULT_PROJECT;
            if (command.IsEmpty) return ExecutionType.SHOW_USAGE;
            if (command.IsVmExportCommand) return ExecutionType.EXPORT_VM_STANDALONE;
            if (command.HasTarget) return ExecutionType.EXPORT_VM_BUNDLE;
            if (command.IsCbxExport) return ExecutionType.EXPORT_CBX;
            return ExecutionType.RUN_CBX;
        }
    }
}
