using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon.Translator
{
	internal abstract class AbstractGamepadTranslator
	{
		public AbstractPlatform Platform { get; set; }
		public AbstractTranslator Translator { get; set; }

		public abstract void TranslateGetAnalogAxisCount(List<string> output, Expression device, Expression analogAxisIndex);
		public abstract void TranslateGetAnalogAxisValue(List<string> output, Expression device, Expression analogAxisIndex);
		public abstract void TranslateGet2dDigitalAxisCount(List<string> output, Expression device, Expression digitalAxisIndex);
		public abstract void TranslateGet2dDigitalAxisValue(List<string> output, Expression device, Expression digitalAxisIndex);
		public abstract void TranslateGetButtonCount(List<string> output, Expression device, Expression buttonIndex);
		public abstract void TranslateGetButtonValue(List<string> output, Expression device, Expression buttonIndex);
		public abstract void TranslateGetDevice(List<string> output, Expression index);
		public abstract void TranslateGetDeviceCount(List<string> output);
		public abstract void TranslateGetDeviceName(List<string> output, Expression nativeDevice, Expression index);
		public abstract void TranslateInitialize(List<string> output, Expression device, Expression gamepadIndex);
		public abstract void TranslatePoll(List<string> output);
	}
}
