using System.Collections.Generic;
using System.Linq;

namespace Wax
{
    public class CryPkgEncoder
    {
        private Dictionary<string, byte[]> files = new Dictionary<string, byte[]>();

        public CryPkgEncoder() { }

        public void AddFile(string filename, byte[] data)
        {
            this.files[filename] = data;
        }

        public void AddTextFile(string filename, string text)
        {
            this.AddFile(filename, this.StringToUtf8(text));
        }

        public byte[] CreateCryPkg()
        {
            string[] paths = this.files.Keys.OrderBy(k => k).ToArray();
            List<byte> headerSection = new List<byte>();
            List<byte> payloadSection = new List<byte>();
            foreach (string path in paths)
            {
                byte[] pathBytes = StringToUtf8(path);
                byte[] fileData = this.files[path];
                headerSection.AddRange(ToBigEndian(pathBytes.Length));
                headerSection.AddRange(ToBigEndian(fileData.Length));
                headerSection.AddRange(ToBigEndian(payloadSection.Count));
                headerSection.AddRange(pathBytes);
                payloadSection.AddRange(fileData);
            }
            List<byte> finalBytes = new List<byte>();
            finalBytes.AddRange(ToBigEndian(this.files.Count));
            finalBytes.AddRange(headerSection);
            finalBytes.AddRange(payloadSection);
            return finalBytes.ToArray();
        }

        private byte[] StringToUtf8(string s)
        {
            return System.Text.Encoding.UTF8.GetBytes(s);
        }

        private byte[] ToBigEndian(int n)
        {
            byte[] output = new byte[4];
            output[0] = (byte)((n >> 24) & 255);
            output[1] = (byte)((n >> 16) & 255);
            output[2] = (byte)((n >> 8) & 255);
            output[3] = (byte)(n & 255);
            return output;
        }
    }
}
