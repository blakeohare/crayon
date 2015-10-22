using System;
using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon.Translator.Python
{
	internal class PyGameGamepadTranslator : AbstractGamepadTranslator
	{
		public override void TranslateGetAnalogAxisCount(List<string> output, Expression device, Expression analogAxisIndex)
		{
			throw new NotImplementedException();
		}

		public override void TranslateGetAnalogAxisValue(List<string> output, Expression device, Expression analogAxisIndex)
		{
			throw new NotImplementedException();
		}

		public override void TranslateGet2dDigitalAxisCount(List<string> output, Expression device, Expression digitalAxisIndex)
		{
			throw new NotImplementedException();
		}

		public override void TranslateGet2dDigitalAxisValue(List<string> output, Expression device, Expression digitalAxisIndex)
		{
			throw new NotImplementedException();
		}

		public override void TranslateGetButtonCount(List<string> output, Expression device, Expression buttonIndex)
		{
			throw new NotImplementedException();
		}

		public override void TranslateGetButtonValue(List<string> output, Expression device, Expression buttonIndex)
		{
			throw new NotImplementedException();
		}

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

		public override void TranslateInitialize(List<string> output, Expression device, Expression gamepadIndex)
		{
			throw new NotImplementedException();
		}

		public override void TranslatePoll(List<string> output)
		{
			throw new NotImplementedException();
		}
	}
}