using Build;
using Common;
using System.Collections.Generic;

namespace Exporter.Workers
{
    public class PopulateFileOutputContextForCbxWorker : AbstractCrayonWorker
    {
        public override CrayonWorkerResult DoWorkImpl(CrayonWorkerResult[] args)
        {
            // PopulateFileOutputContext(fileOutputContext, buildContext, resDb, cbxFileBytes)
            Dictionary<string, FileOutput> fileOutputDescriptor = (Dictionary<string, FileOutput>)args[0].Value;
            BuildContext buildContext = (BuildContext)args[1].Value;
            ResourceDatabase resDb = (ResourceDatabase)args[2].Value;
            byte[] cbxData = (byte[])args[3].Value;
            this.GenerateFileOutput(fileOutputDescriptor, buildContext, resDb, cbxData);
            return new CrayonWorkerResult();
        }

        private Dictionary<string, FileOutput> GenerateFileOutput(
            Dictionary<string, FileOutput> output,
            BuildContext buildContext,
            ResourceDatabase resDb,
            byte[] cbxData)
        {
            output[buildContext.ProjectID + ".cbx"] = new FileOutput()
            {
                Type = FileOutputType.Binary,
                BinaryContent = cbxData,
            };

            // Resource manifest and image sheet manifest is embedded into the CBX file

            foreach (FileOutput txtResource in resDb.TextResources)
            {
                output["res/txt/" + txtResource.CanonicalFileName] = txtResource;
            }
            foreach (FileOutput sndResource in resDb.AudioResources)
            {
                output["res/snd/" + sndResource.CanonicalFileName] = sndResource;
            }
            foreach (FileOutput fontResource in resDb.FontResources)
            {
                output["res/ttf/" + fontResource.CanonicalFileName] = fontResource;
            }
            foreach (FileOutput binResource in resDb.BinaryResources)
            {
                output["res/bin/" + binResource.CanonicalFileName] = binResource;
            }
            foreach (FileOutput imgResource in resDb.ImageResources)
            {
                output["res/img/" + imgResource.CanonicalFileName] = imgResource;
            }
            foreach (string key in resDb.ImageSheetFiles.Keys)
            {
                output["res/img/" + key] = resDb.ImageSheetFiles[key];
            }

            return output;
        }
    }
}
