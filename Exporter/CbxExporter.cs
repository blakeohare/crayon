using Build;
using Common;
using Parser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Crayon
{
    public class CbxExporter
    {
        private ExportCommand args;
        private string finalCbxPath;

        public CbxExporter(ExportCommand args)
        {
            this.args = args;
        }

        public CbxExporter Export()
        {
            using (new PerformanceSection("ExportCbx"))
            {
                if (!this.args.HasBuildFile)
                {
                    throw new InvalidOperationException("No build path was provided.");
                }

                BuildContext buildContext = GetBuildContextCbx(this.args.BuildFilePath);
                CompilationBundle compilationResult = CompilationBundle.Compile(buildContext);
                ResourceDatabase resDb = ResourceDatabaseBuilder.PrepareResources(buildContext, null);
                string byteCode = ByteCodeEncoder.Encode(compilationResult.ByteCode);
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

                string outputFolder = buildContext.OutputFolder.Replace("%TARGET_NAME%", "cbx");
                string fullyQualifiedOutputFolder = FileUtil.JoinPath(buildContext.ProjectDirectory, outputFolder);
                string cbxPath = FileUtil.JoinPath(fullyQualifiedOutputFolder, buildContext.ProjectID + ".cbx");
                cbxPath = FileUtil.GetCanonicalizeUniversalPath(cbxPath);
                FileUtil.EnsureParentFolderExists(fullyQualifiedOutputFolder);
                Dictionary<string, FileOutput> output = new Dictionary<string, FileOutput>();
                output[buildContext.ProjectID + ".cbx"] = new FileOutput()
                {
                    Type = FileOutputType.Binary,
                    BinaryContent = cbxOutput.ToArray(),
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
                new FileOutputExporter(fullyQualifiedOutputFolder).ExportFiles(output);

                this.finalCbxPath = cbxPath;
            }
            return this;
        }

        private static BuildContext GetBuildContextCbx(string rawBuildFilePath)
        {
            using (new PerformanceSection("GetBuildContextCbx"))
            {
                string buildFile = BuildContext.GetValidatedCanonicalBuildFilePath(rawBuildFilePath);
                string projectDirectory = System.IO.Path.GetDirectoryName(buildFile);
                string buildFileContent = System.IO.File.ReadAllText(buildFile);
                return BuildContext.Parse(projectDirectory, buildFileContent, null);
            }
        }

        public string GetCbxPath()
        {
            return this.finalCbxPath;
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
