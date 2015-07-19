using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon.Translator.Java
{
	internal class JavaAndroidPlatform : JavaPlatform
	{
		public JavaAndroidPlatform()
			: base(new JavaAndroidSystemFunctionTranslator())
		{
		}

		public override bool IsOpenGlBased { get { return true; } }
	}
}
