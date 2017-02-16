using System;
using System.Collections.Generic;
using System.Linq;
using Common;

namespace Crayon
{
    internal class FileOutputExporter
    {
        private string targetDirectory;

        public FileOutputExporter(string targetDirectory)
        {
            this.targetDirectory = targetDirectory;
            FileUtil.EnsureFolderExists(targetDirectory);
        }

        public void ExportFiles(Dictionary<string, FileOutput> files)
        {
            foreach (string file in files.Keys.OrderBy(s => s)) // deterministic order
            {
                this.ExportFile(file, files[file]);
            }
        }

        private void ExportFile(string path, FileOutput file)
        {
            string absolutePath = FileUtil.JoinPath(this.targetDirectory, path);
            if (file.Type != FileOutputType.Ghost)
            {
                FileUtil.EnsureParentFolderExists(absolutePath);
            }

            switch (file.Type)
            {
                case FileOutputType.Binary:
                    this.ExportBinaryFile(absolutePath, file.BinaryContent);
                    break;

                case FileOutputType.Copy:
                    this.ExportCopiedFile(absolutePath, file.AbsoluteInputPath);
                    break;

                case FileOutputType.Image:
                    this.ExportImageFile(absolutePath, file.Bitmap);
                    break;

                case FileOutputType.Text:
                    this.ExportTextFile(absolutePath, file.TextContent);
                    break;

                case FileOutputType.Ghost:
                    // do nothing.
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private void ExportBinaryFile(string path, byte[] content)
        {
            FileUtil.WriteFileBytes(path, content);
        }

        private void ExportCopiedFile(string path, string originalAbsolutePath)
        {
            FileUtil.CopyFile(originalAbsolutePath, path);
        }

        private void ExportImageFile(string path, SystemBitmap image)
        {
            image.Save(path);
        }

        private void ExportTextFile(string path, string content)
        {
            FileUtil.WriteFileText(path, content);
        }
    }
}
