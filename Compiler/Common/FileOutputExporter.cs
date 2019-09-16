using CommonUtil.Disk;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Common
{
    public class FileOutputExporter
    {
        private string targetDirectory;

        private static readonly HashSet<string> BOMLESS_TEXT_TYPES = new HashSet<string>() { "java" };

        public FileOutputExporter(string targetDirectory)
        {
            this.targetDirectory = targetDirectory;
            FileUtil.EnsureFolderExists(targetDirectory);
        }

        public void ExportFiles(Dictionary<string, FileOutput> files)
        {
            using (new PerformanceSection("FileOutputExporter.ExportFiles", true))
            {
                foreach (string file in files.Keys.OrderBy(s => s)) // deterministic order
                {
                    this.ExportFile(file, files[file]);
                }
            }
        }

        private void ExportFile(string path, FileOutput file)
        {
            string absolutePath;
            using (new PerformanceSection("JoinPath"))
            {
                absolutePath = FileUtil.JoinPath(this.targetDirectory, path);
            }

            using (new PerformanceSection("EnsureParentFolderExists"))
            {
                if (file.Type != FileOutputType.Ghost)
                {
                    FileUtil.EnsureParentFolderExists(absolutePath);
                }
            }

            switch (file.Type)
            {
                case FileOutputType.Binary:
                    this.ExportBinaryFile(absolutePath, file.BinaryContent);
                    break;

                case FileOutputType.Copy:
                case FileOutputType.Move:
                    this.ExportCopiedFile(absolutePath, file.AbsoluteInputPath, file.Type == FileOutputType.Move);
                    break;

                case FileOutputType.Image:
                    this.ExportImageFile(absolutePath, file.Bitmap);
                    break;

                case FileOutputType.Text:
                    this.ExportTextFile(absolutePath, file.TextContent, file.TrimBomIfPresent);
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
            using (new PerformanceSection("ExportBinaryFile"))
            {
                FileUtil.WriteFileBytes(path, content);
            }
        }

        private void ExportCopiedFile(string path, string originalAbsolutePath, bool isMove)
        {
            using (new PerformanceSection("ExportCopiedFile"))
            {
                if (isMove)
                {
                    FileUtil.MoveFile(originalAbsolutePath, path, true);
                }
                else
                {
                    FileUtil.CopyFile(originalAbsolutePath, path);
                }
            }
        }

        private void ExportImageFile(string path, SystemBitmap image)
        {
            using (new PerformanceSection("ExportImageFile"))
            {
                image.Save(path);
            }
        }

        private void ExportTextFile(string path, string content, bool trimBom)
        {
            string fileExtension = FileUtil.GetCanonicalExtension(path);
            if (trimBom ||
                (fileExtension != null && BOMLESS_TEXT_TYPES.Contains(fileExtension)))
            {
                using (new PerformanceSection("ExportTextFileWithBytes"))
                {
                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(content);
                    this.ExportBinaryFile(path, bytes);
                }
            }
            else
            {
                using (new PerformanceSection("ExportTextFileStraightText"))
                {
                    FileUtil.WriteFileText(path, content);
                }
            }
        }
    }
}
