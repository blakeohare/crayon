using Build;
using Common;
using System.Collections.Generic;

namespace Exporter.Workers
{
    public class PopulateFileOutputContextForCbxWorker
    {
        public void GenerateFileOutput(
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
        }
    }
}
