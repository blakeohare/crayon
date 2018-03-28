using Build;
using Common;
using Parser;
using System.Collections.Generic;
using System.Linq;

namespace Exporter
{
    public class CbxExporter
    {
        public string FinalCbxPath { get; private set; }

        public CbxExporter Export(BuildContext buildContext)
        {
            using (new PerformanceSection("ExportCbx"))
            {
                // TODO: convert this to a Worker (maybe? so that output folder is usable in pipeline)
                string outputFolder = buildContext.OutputFolder.Replace("%TARGET_NAME%", "cbx");
                string fullyQualifiedOutputFolder = FileUtil.JoinPath(buildContext.ProjectDirectory, outputFolder);

                // TODO: convert this to a Worker
                CompilationBundle compilationResult = CompilationBundle.Compile(buildContext);

                // TODO: convert this to a Worker
                ResourceDatabase resDb = ResourceDatabaseBuilder.PrepareResources(buildContext, null);

                // TODO: convert this to a Worker
                string byteCode = ByteCodeEncoder.Encode(compilationResult.ByteCode);

                // TODO: convert this to a Worker
                byte[] cbxData = this.GenerateCbxBinaryData(buildContext, resDb, compilationResult, byteCode);

                Dictionary<string, FileOutput> fileOutputDescriptor = new Dictionary<string, FileOutput>();

                // TODO: convert this to a Worker
                this.GenerateFileOutput(fileOutputDescriptor , buildContext, resDb, cbxData);

                // TODO: convert this to a Worker (non-specific to CBX)
                FileUtil.EnsureParentFolderExists(fullyQualifiedOutputFolder);
                new FileOutputExporter(fullyQualifiedOutputFolder).ExportFiles(fileOutputDescriptor);

                // TODO: convert this to a Worker where FinalCbxPath is actually returned
                // or possibly attach it to the ExportCommand.
                string cbxPath = FileUtil.JoinPath(fullyQualifiedOutputFolder, buildContext.ProjectID + ".cbx");
                cbxPath = FileUtil.GetCanonicalizeUniversalPath(cbxPath);

                this.FinalCbxPath = cbxPath;
            }
            return this;
        }

        private byte[] GenerateCbxBinaryData(BuildContext buildContext, ResourceDatabase resDb, CompilationBundle compilationResult, string byteCode)
        {
            List<byte> cbxOutput = new List<byte>() { 0 };
            cbxOutput.AddRange("CBX".ToCharArray().Select(c => (byte)c));
            cbxOutput.AddRange(GetBigEndian4Byte(0));
            cbxOutput.AddRange(GetBigEndian4Byte(2));
            cbxOutput.AddRange(GetBigEndian4Byte(0));

            byte[] code = StringToBytes(byteCode);
            cbxOutput.AddRange("CODE".ToCharArray().Select(c => (byte)c));
            cbxOutput.AddRange(GetBigEndian4Byte(code.Length));
            cbxOutput.AddRange(code);

            List<string> libraries = new List<string>();
            foreach (LibraryCompilationScope scopeForLibrary in compilationResult.LibraryScopesUsed.Where(scope => scope.Library.IsMoreThanJustEmbedCode))
            {
                libraries.Add(scopeForLibrary.Library.ID);
                libraries.Add(scopeForLibrary.Library.Version);
            }
            string libsData = string.Join(",", libraries);
            byte[] libsDataBytes = StringToBytes(libsData);
            cbxOutput.AddRange("LIBS".ToCharArray().Select(c => (byte)c));
            cbxOutput.AddRange(GetBigEndian4Byte(libsDataBytes.Length));
            cbxOutput.AddRange(libsDataBytes);

            byte[] resourceManifest = StringToBytes(resDb.ResourceManifestFile.TextContent);
            cbxOutput.AddRange("RSRC".ToCharArray().Select(c => (byte)c));
            cbxOutput.AddRange(GetBigEndian4Byte(resourceManifest.Length));
            cbxOutput.AddRange(resourceManifest);

            if (resDb.ImageSheetManifestFile != null)
            {
                byte[] imageSheetManifest = StringToBytes(resDb.ImageSheetManifestFile.TextContent);
                cbxOutput.AddRange("IMSH".ToCharArray().Select(c => (byte)c));
                cbxOutput.AddRange(GetBigEndian4Byte(imageSheetManifest.Length));
                cbxOutput.AddRange(imageSheetManifest);
            }

            return cbxOutput.ToArray();
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

        private static byte[] StringToBytes(string value)
        {
            return System.Text.Encoding.UTF8.GetBytes(value);
        }

        private static byte[] GetBigEndian4Byte(int value)
        {
            return new byte[]
            {
                (byte) ((value >> 24) & 255),
                (byte) ((value >> 16) & 255),
                (byte) ((value >> 8) & 255),
                (byte) (value & 255)
            };
        }
    }
}
