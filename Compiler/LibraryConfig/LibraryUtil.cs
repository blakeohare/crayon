using System.Collections.Generic;
using System.Text;

namespace LibraryConfig
{
	public static class LibraryUtil
	{
		private static readonly byte[] BUFFER = new byte[1000];

		// TODO: replace the Crayon.Util implementation with this.
		public static string ReadEmbeddedTextResource(System.Reflection.Assembly assembly, string path)
		{
			byte[] bytes = LibraryUtil.ReadEmbeddedResource(assembly, path);
			int i = 0;
			if (bytes.Length >= 3 &&
				bytes[0] == 239 &&
				bytes[1] == 187 &&
				bytes[2] == 191)
			{
				i = 3;
			}

			StringBuilder sb = new StringBuilder();
			int length = bytes.Length;
			while (i < length)
			{
				sb.Append((char)bytes[i++]);
			}
			return sb.ToString();
		}

		public static byte[] ReadEmbeddedResource(System.Reflection.Assembly assembly, string path)
		{
			System.IO.Stream stream = assembly.GetManifestResourceStream(assembly.GetName().Name + "." + path.Replace('/', '.'));
			if (stream == null)
			{
				throw new System.Exception(path + " not marked as an embedded resource.");
			}
			List<byte> output = new List<byte>();
			int bytesRead = 1;
			while (bytesRead > 0)
			{
				bytesRead = stream.Read(BUFFER, 0, BUFFER.Length);
				if (bytesRead == BUFFER.Length)
				{
					output.AddRange(BUFFER);
				}
				else
				{
					for (int i = 0; i < bytesRead; ++i)
					{
						output.Add(BUFFER[i]);
					}
					bytesRead = 0;
				}
			}

			return output.ToArray();
		}
	}
}
