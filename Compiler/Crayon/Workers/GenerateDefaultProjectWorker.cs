using Common;
using Exporter;
using System;
using System.Collections.Generic;

namespace Crayon
{
    public class GenerateDefaultProjectWorker : AbstractCrayonWorker
    {
        public override CrayonWorkerResult DoWorkImpl(CrayonWorkerResult[] args)
        {
            ExportCommand command = (ExportCommand)args[0].Value;
            DefaultProjectGenerator generator = new DefaultProjectGenerator(command.DefaultProjectId, command.DefaultProjectLocale);
            Dictionary<string, FileOutput> project = generator.Validate().Export();

            string directory = FileUtil.JoinPath(
                FileUtil.GetCurrentDirectory(),
                generator.ProjectID);
            new FileOutputExporter(directory).ExportFiles(project);

            Console.WriteLine("Empty project exported to directory '" + generator.ProjectID + "/'");
            return null;
        }
    }
}
