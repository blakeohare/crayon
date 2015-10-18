using System;
using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon.Translator.Python
{
	internal class PyGameGamepadTranslator : AbstractGamepadTranslator
	{
		public override void TranslateGetDevice(List<string> output, Expression index)
		{
			throw new NotImplementedException();
		}

		public override void TranslateGetDeviceCount(List<string> output)
		{
			throw new NotImplementedException();
		}

		public override void TranslateGetDeviceName(List<string> output, Expression nativeDevice, Expression index)
		{
			throw new NotImplementedException();
		}
	}
}
