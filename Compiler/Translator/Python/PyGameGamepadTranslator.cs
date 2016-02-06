using System;
using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon.Translator.Python
{
	internal class PyGameGamepadTranslator : AbstractGamepadTranslator
	{
		public override void TranslateGetAnalogAxisCount(List<string> output, Expression device, Expression analogAxisIndex)
		{
			this.Translator.TranslateExpression(output, device);
			output.Add(".get_numaxes()");
		}

		public override void TranslateGetAnalogAxisValue(List<string> output, Expression device, Expression analogAxisIndex)
		{
			this.Translator.TranslateExpression(output, device);
			output.Add(".get_axis(");
			this.Translator.TranslateExpression(output, analogAxisIndex);
			output.Add(")");
		}

		public override void TranslateGet2dDigitalAxisCount(List<string> output, Expression device, Expression digitalAxisIndex)
		{
			this.Translator.TranslateExpression(output, device);
			output.Add(".get_numhats()");
		}

		public override void TranslateGet2dDigitalAxisValue(List<string> output, Expression device, Expression digitalAxisIndex)
		{
			this.Translator.TranslateExpression(output, device);
			output.Add(".get_hat(");
			this.Translator.TranslateExpression(output, digitalAxisIndex);
			output.Add(")");
		}

		public override void TranslateGetButtonCount(List<string> output, Expression device, Expression buttonIndex)
		{
			this.Translator.TranslateExpression(output, device);
			output.Add(".get_numbuttons()");
		}

		public override void TranslateGetButtonValue(List<string> output, Expression device, Expression buttonIndex)
		{
			this.Translator.TranslateExpression(output, device);
			output.Add(".get_button(");
			this.Translator.TranslateExpression(output, buttonIndex);
			output.Add(")");
		}

		public override void TranslateGetDevice(List<string> output, Expression index)
		{
			output.Add("pygame.joystick.Joystick(");
			this.Translator.TranslateExpression(output, index);
			output.Add(")");
		}

		public override void TranslateGetDeviceCount(List<string> output)
		{
			output.Add("pygame.joystick.get_count()");
		}

		public override void TranslateGetDeviceName(List<string> output, Expression nativeDevice, Expression index)
		{
			this.Translator.TranslateExpression(output, nativeDevice);
			output.Add(".get_name()");
		}

		public override void TranslateInitialize(List<string> output, Expression device, Expression gamepadIndex)
		{
			this.Translator.TranslateExpression(output, device);
			output.Add(".init()");
		}

		public override void TranslatePoll(List<string> output)
		{
			output.Add("pass");
		}
	}
}