using System.Collections.Generic;
using Common;

namespace JavaScriptAppChrome
{
    public static class ZipCreator
    {
        public static FileOutput Create(Dictionary<string, FileOutput> files, bool storeInMemory)
        {
            string tmpNameSeed = GuidHelper.GetRandomSeed();
            string tmpDirRoot = System.IO.Path.GetTempPath();
            string tempDir = System.IO.Path.Combine(tmpDirRoot, GuidHelper.GenerateTempDirName(tmpNameSeed, "zip-intermediate-dir"));
            string zipPath = System.IO.Path.Combine(tmpDirRoot, GuidHelper.GenerateTempDirName(tmpNameSeed, "zip-output") + ".zip");
            new FileOutputExporter(tempDir).ExportFiles(files);
            System.IO.Compression.ZipFile.CreateFromDirectory(tempDir, zipPath);
            FileUtil.DirectoryDelete(tempDir);
            if (storeInMemory)
            {
                FileOutput output = new FileOutput()
                {
                    Type = FileOutputType.Binary,
                    BinaryContent = System.IO.File.ReadAllBytes(zipPath),
                };
                System.IO.File.Delete(zipPath);
                return output;
            }
            else
            {
                return new FileOutput()
                {
                    Type = FileOutputType.Move,
                    AbsoluteInputPath = zipPath,
                };
            }
        }
    }
}
