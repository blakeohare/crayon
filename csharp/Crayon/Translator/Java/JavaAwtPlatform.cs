using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon.Translator.Java
{
	internal class JavaAwtPlatform : JavaPlatform
	{
		public JavaAwtPlatform()
			: base(new JavaAwtSystemFunctionTranslator())
		{
		}

		public override bool IsOpenGlBased { get { return false; } }
	}
}
