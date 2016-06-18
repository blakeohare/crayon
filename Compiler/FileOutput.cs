using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon
{
	enum FileOutputType
	{
		Text,
		Binary,
		Copy,
		Image
	}

	class FileOutput
	{
		public FileOutputType Type { get; set; }

		public string RelativeInputPath { get; set; }
		public string TextContent { get; set; }
		public byte[] BinaryContent { get; set; }
		public SystemBitmap Bitmap { get; set; }
	}
}
