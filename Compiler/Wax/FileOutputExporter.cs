using System;
using System.Collections.Generic;
using System.Linq;
using Wax.Util.Disk;
using Wax.Util.Images;

namespace Wax
{
    public class FileOutputExporter
    {
        private string targetDirectory;

        private static readonly HashSet<string> BOMLESS_TEXT_TYPES = new HashSet<string>() { "java" };

        public FileOutputExporter(string targetDirectory)
        {
            this.targetDirectory = targetDirectory;
            FileUtil.EnsureFolderExists_DEPRECATED(targetDirectory);
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
            string absolutePath = DiskUtil.JoinPathNative(this.targetDirectory, path);

            FileUtil.EnsureParentFolderExists_DEPRECATED(absolutePath);

            switch (file.Type)
            {
                case FileOutputType.Binary:
                    this.ExportBinaryFile(absolutePath, file.BinaryContent);
                    break;

                case FileOutputType.Copy:
                    this.ExportCopiedFile(absolutePath, file.AbsoluteInputPath, false);
                    break;

                case FileOutputType.Image:
                    if (file.BinaryContent != null)
                    {
                        this.ExportBinaryFile(absolutePath, file.BinaryContent);
                    }
                    else if (file.BinaryContentB64 != null)
                    {
                        this.ExportBinaryFile(absolutePath, file.GetFinalBinaryContent());
                    }
                    else
                    {
                        this.ExportImageFile(absolutePath, file.Bitmap);
                    }
                    break;

                case FileOutputType.Text:
                    this.ExportTextFile(absolutePath, file.TextContent, file.TrimBomIfPresent);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private void ExportBinaryFile(string path, byte[] content)
        {
            FileUtil.WriteFileBytes_DEPRECATED(path, content);
        }

        private void ExportCopiedFile(string path, string originalAbsolutePath, bool isMove)
        {
            if (isMove)
            {
                File.Move(originalAbsolutePath, path, true);
            }
            else
            {
                File.Copy(originalAbsolutePath, path);
            }
        }

        private void ExportImageFile(string path, UniversalBitmap image)
        {
            image.Save(path);
        }

        private void ExportTextFile(string path, string content, bool trimBom)
        {
            string fileExtension = FileUtil.GetCanonicalExtension(path);
            if (trimBom ||
                (fileExtension != null && BOMLESS_TEXT_TYPES.Contains(fileExtension)))
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(content);
                this.ExportBinaryFile(path, bytes);
            }
            else
            {
                FileUtil.WriteFileText_DEPRECATED(path, content);
            }
        }
    }
}
