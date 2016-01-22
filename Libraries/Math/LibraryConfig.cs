using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crayon;

namespace Math
{
	public class LibraryConfig : ILibraryConfig
	{
		private static string ReadFile(string path)
		{
			return Util.ReadFileInternally(typeof(LibraryConfig).Assembly, path);
		}

		public string GetEmbeddedCode()
		{
			return ReadFile("embed.cry");
		}
	}
}
