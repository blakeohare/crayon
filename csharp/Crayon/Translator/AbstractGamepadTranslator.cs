using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon.Translator
{
	internal abstract class AbstractGamepadTranslator
	{
		public AbstractPlatform Platform { get; set; }
		public AbstractTranslator Translator { get; set; }

		public abstract void TranslateGetDevice(List<string> output, Expression index);
		public abstract void TranslateGetDeviceCount(List<string> output);
		public abstract void TranslateGetDeviceName(List<string> output, Expression nativeDevice, Expression index);
	}
}
