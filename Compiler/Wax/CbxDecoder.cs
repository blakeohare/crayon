﻿using System.Collections.Generic;

namespace Wax
{
    public class CbxDecoder
    {
        private CryPkgDecoder decoder;

        public CbxDecoder(byte[] cbxFileBytes)
        {
            this.decoder = new CryPkgDecoder(cbxFileBytes);
        }

        private Dictionary<string, string> cachedTextFiles = new Dictionary<string, string>();

        private string GetTextFile(string path)
        {
            if (!this.cachedTextFiles.ContainsKey(path))
            {
                this.cachedTextFiles[path] = this.decoder.ReadFileString(path);
            }
            return this.cachedTextFiles[path];
        }

        public string ByteCode { get { return this.GetTextFile("bytecode.txt"); } }
        public string ResourceManifest { get { return this.GetTextFile("manifest.txt"); } }
        public string ImageManifest { get { return this.GetTextFile("images.txt"); } }

        public string[] ResourceNames
        {
            get
            {
                if (!this.decoder.DirectoryExists("res")) return new string[0];
                return this.decoder.ListDirectory("res", true, false);
            }
        }

        public byte[] GetResourceBytes(string name)
        {
            return this.decoder.ReadFileBytes("res/" + name);
        }
    }
}
