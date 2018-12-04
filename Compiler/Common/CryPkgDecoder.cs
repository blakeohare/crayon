using System;
using System.Collections.Generic;
using System.Linq;

namespace Common
{
    /*
        CryPkg format: like TAR but much much simpler

        (all numbers are in big endian)

        4 bytes --> size of header in bytes
        4 bytes --> file count

        for each file:
            4 bytes --> path length
            4 bytes --> file size
            4 bytes --> offset into payload
            n bytes --> path in utf8

        payload area:
            just a bunch of bytes
    */
    public class CryPkgDecoder
    {
        private class FileInfo
        {
            public string Path { get; set; }
            public int DataOffset { get; set; }
            public int Length { get; set; }
            public bool IsDirectory { get; set; }
            public HashSet<string> Children { get; set; } // for directories
        }

        private byte[] data;
        private int payloadOffset;
        private Dictionary<string, FileInfo> files = null;

        public CryPkgDecoder(byte[] bytes)
        {
            this.data = bytes;
            this.Init();
        }

        private void Init()
        {
            int fileCount = ReadInt32(0);
            int index = 4;
            for (int i = 0; i < fileCount; ++i)
            {
                int pathLength = ReadInt32(index);
                int fileSize = ReadInt32(index + 4);
                int byteOffset = ReadInt32(index + 8);
                string path = ReadString(index + 12, pathLength);
                index += 12 + pathLength;
                this.files[path] = new FileInfo()
                {
                    Children = null,
                    DataOffset = byteOffset,
                    IsDirectory = false,
                    Length = fileSize,
                    Path = path,
                };
            }
            this.payloadOffset = index;
            this.PopulateDirectories();
        }

        public bool FileExists(string path)
        {
            return this.files.ContainsKey(path) && !this.files[path].IsDirectory;
        }

        public bool DirectoryExists(string path)
        {
            return this.files.ContainsKey(path) && this.files[path].IsDirectory;
        }

        public byte[] ReadFileBytes(string path)
        {
            if (!this.files.ContainsKey(path) || this.files[path].IsDirectory) throw new InvalidOperationException();
            FileInfo file = this.files[path];
            int offset = file.DataOffset + this.payloadOffset;
            byte[] output = new byte[file.Length];
            Array.Copy(this.data, offset, output, 0, file.Length);
            return output;
        }

        public string ReadFileString(string path)
        {
            byte[] bytes = this.ReadFileBytes(path);
            return MysteryTextDecoder.DecodeArbitraryBytesAsAppropriatelyAsPossible(bytes);
        }

        public string[] ListDirectory(string path)
        {
            if (!this.files.ContainsKey(path) || !this.files[path].IsDirectory) throw new InvalidOperationException();
            int trimLength = path.Length + 1;
            return this.files[path].Children
                .Select(s => s.Substring(trimLength))
                .OrderBy(t => t)
                .ToArray();
        }

        private void PopulateDirectories()
        {
            this.files["."] = new FileInfo()
            {
                Children = new HashSet<string>(),
                IsDirectory = true,
                Path = ".",
            };

            foreach (string filePath in this.files.Keys)
            {
                string current = filePath;
                string dir = this.GetParentDir(filePath);
                while (true)
                {
                    if (this.files.ContainsKey(dir))
                    {
                        this.files[dir].Children.Add(current);
                        break;
                    }
                    else
                    {
                        this.files[dir] = new FileInfo()
                        {
                            Children = new HashSet<string>() { current },
                            IsDirectory = true,
                            Path = dir,
                        };
                    }
                    current = dir;
                    dir = GetParentDir(dir);
                }
            }
        }

        private string GetParentDir(string path)
        {
            int slash = path.LastIndexOf('/');
            if (slash == -1) return ".";
            return path.Substring(0, slash);
        }

        // don't worry about negatives, these are all positive offsets and sizes.
        private int ReadInt32(int index)
        {
            int value = 0;
            value |= this.data[index] << 24;
            value |= this.data[index + 1] << 16;
            value |= this.data[index + 2] << 8;
            value |= this.data[index + 3] + 0;
            return value;
        }

        private string ReadString(int index, int length)
        {
            return System.Text.Encoding.UTF8.GetString(this.data, index, length);
        }
    }
}
