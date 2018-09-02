using Exporter;

namespace Crayon
{
    public enum ExecutionType
    {
        GENERATE_DEFAULT_PROJECT,
        EXPORT_VM_BUNDLE,
        EXPORT_VM_STANDALONE,
        EXPORT_CBX,
        RUN_CBX,
        SHOW_USAGE,
    }

    internal class TopLevelCheckWorker
    {
        public ExportCommand DoWorkImpl()
        {
            string[] commandLineArgs = Program.GetCommandLineArgs();

            ExportCommand command = FlagParser.Parse(commandLineArgs);

            // TODO: I don't like these here.
            command.PlatformProvider = new PlatformProvider();
            command.InlineImportCodeLoader = new InlineImportCodeLoader();

            return command;
        }

        public static ExecutionType IdentifyUseCase(ExportCommand command)
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
