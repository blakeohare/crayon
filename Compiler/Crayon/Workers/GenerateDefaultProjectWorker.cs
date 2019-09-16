using Common;
using CommonUtil.Disk;
using Exporter;
using System.Collections.Generic;

namespace Crayon
{
    public class GenerateDefaultProjectWorker
    {
        public void DoWorkImpl(ExportCommand command)
        {
            DefaultProjectGenerator generator = new DefaultProjectGenerator(command.DefaultProjectId, command.DefaultProjectLocale);
            Dictionary<string, FileOutput> project = generator.Validate().Export();

            string directory = FileUtil.JoinPath(
                FileUtil.GetCurrentDirectory(),
                generator.ProjectID);
            new FileOutputExporter(directory).ExportFiles(project);

            ConsoleWriter.Print(
                ConsoleMessageType.DEFAULT_PROJ_EXPORT_INFO,
                "Empty project exported to directory '" + generator.ProjectID + "/'");
        }
    }
}
