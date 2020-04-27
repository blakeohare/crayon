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
        SHOW_VERSION,
        ERROR_CHECK_ONLY,
        TRANSPILE_CSHARP_TO_ACRYLIC,
    }

    internal class TopLevelCheckWorker
    {

        public static ExecutionType IdentifyUseCase(Command command)
        {
            if (command.ShowVersion) return ExecutionType.SHOW_VERSION;
            if (command.IsCSharpToAcrylicTranspiler) return ExecutionType.TRANSPILE_CSHARP_TO_ACRYLIC;
            if (command.IsGenerateDefaultProject) return ExecutionType.GENERATE_DEFAULT_PROJECT;
            if (command.IsEmpty) return ExecutionType.SHOW_USAGE;
            if (command.IsErrorCheckOnly) return ExecutionType.ERROR_CHECK_ONLY;
            if (command.IsVmExportCommand) return ExecutionType.EXPORT_VM_STANDALONE;
            if (command.HasTarget) return ExecutionType.EXPORT_VM_BUNDLE;
            if (command.IsCbxExport) return ExecutionType.EXPORT_CBX;
            return ExecutionType.RUN_CBX;
        }
    }
}
