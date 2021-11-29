﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Runtime
{
    internal class CbxDecoder
    {
        private Dictionary<string, string> chunks = new Dictionary<string, string>();
        private int vMajor;
        private int vMinor;
        private int vRelease;

        public CbxDecoder(byte[] data)
        {
            if (data.Length < 4 ||
                data[0] != 0 ||
                this.GetString(data, 1, 3) != "CBX")
            {
                throw InvalidFile();
            }

            this.vMajor = this.GetFourByteBigEndianNumber(data, 4);
            this.vMinor = this.GetFourByteBigEndianNumber(data, 8);
            this.vRelease = this.GetFourByteBigEndianNumber(data, 12);

            int i = 16;
            while (i < data.Length)
            {
                if (i + 8 > data.Length) throw InvalidFile();
                string chunkId = this.GetString(data, i, 4);
                int chunkLength = this.GetFourByteBigEndianNumber(data, i + 4);
                i += 8;
                if (i + chunkLength > data.Length)
                {
                    throw InvalidFile();
                }

                string chunkData = this.GetString(data, i, chunkLength);
                i += chunkLength;
                if (this.chunks.ContainsKey(chunkId)) throw InvalidFile();
                this.chunks[chunkId] = chunkData;
            }
        }

        public string ByteCode { get { return this.GetChunk("CODE"); } }
        public string ResourceManifest { get { return this.GetChunk("RSRC") ?? ""; } }
        public string ImageManifest { get { return this.GetChunk("IMGS") ?? ""; } }
        public string CbxVersion { get { return this.vMajor + "." + this.vMinor + "." + this.vRelease; } }

        private int GetFourByteBigEndianNumber(byte[] data, int index)
        {
            if (index + 4 > data.Length) throw InvalidFile();
            int output = 0;
            for (int i = 0; i < 4; ++i)
            {
                output = (output << 8) | data[index + i];
            }
            return output;
        }

        private string GetString(byte[] data, int index, int length)
        {
            if (index + length > data.Length) throw InvalidFile();
            try
            {
                return UTF8Encoding.UTF8.GetString(data, index, length);
            }
            catch (Exception)
            {
                throw InvalidFile();
            }
        }

        private string GetChunk(string id)
        {
            string content;
            return chunks.TryGetValue(id, out content) ? content : null;
        }

        private Exception InvalidFile()
        {
            return new Exception("CBX file is invalid.");
        }
    }
}
