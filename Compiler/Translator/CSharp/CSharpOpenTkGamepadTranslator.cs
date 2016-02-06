using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon.Translator.CSharp
{
	internal class CSharpOpenTkGamepadTranslator : AbstractGamepadTranslator
	{
		public override void TranslateGetAnalogAxisCount(List<string> output, Expression device, Expression analogAxisIndex)
		{
			output.Add("GamepadTranslationHelper.GetAnalogAxisCount(");
			this.Translator.TranslateExpression(output, device);
			output.Add(")");
		}

		public override void TranslateGetAnalogAxisValue(List<string> output, Expression device, Expression analogAxisIndex)
		{
			output.Add("GamepadTranslationHelper.GetAnalogAxis(");
			this.Translator.TranslateExpression(output, device);
			output.Add(", ");
			this.Translator.TranslateExpression(output, analogAxisIndex);
			output.Add(")");
		}

		public override void TranslateGet2dDigitalAxisCount(List<string> output, Expression device, Expression digitalAxisIndex)
		{
			output.Add("GamepadTranslationHelper.GetDigitalAxisCount(");
			this.Translator.TranslateExpression(output, device);
			output.Add(")");
		}

		public override void TranslateGet2dDigitalAxisValue(List<string> output, Expression device, Expression digitalAxisIndex)
		{
			output.Add("GamepadTranslationHelper.GetDigitalAxis(");
			this.Translator.TranslateExpression(output, device);
			output.Add(", ");
			this.Translator.TranslateExpression(output, digitalAxisIndex);
			output.Add(")");
		}

		public override void TranslateGetButtonCount(List<string> output, Expression device, Expression buttonIndex)
		{
			output.Add("GamepadTranslationHelper.GetButtonCount(");
			this.Translator.TranslateExpression(output, device);
			output.Add(")");
		}

		public override void TranslateGetButtonValue(List<string> output, Expression device, Expression buttonIndex)
		{
			output.Add("GamepadTranslationHelper.GetButtonValue(");
			this.Translator.TranslateExpression(output, device);
			output.Add(", ");
			this.Translator.TranslateExpression(output, buttonIndex);
			output.Add(")");
		}

		public override void TranslateGetDevice(List<string> output, Expression index)
		{
			output.Add("GamepadTranslationHelper.GetDevice(");
			this.Translator.TranslateExpression(output, index);
			output.Add(")");
		}

		public override void TranslateGetDeviceCount(List<string> output)
		{
			output.Add("GamepadTranslationHelper.GetDeviceCount()");
		}

		public override void TranslateGetDeviceName(List<string> output, Expression nativeDevice, Expression index)
		{
			output.Add("GamepadTranslationHelper.GetDeviceName(");
			this.Translator.TranslateExpression(output, nativeDevice);
			output.Add(")");
		}

		public override void TranslateInitialize(List<string> output, Expression device, Expression gamepadIndex)
		{
			output.Add("GamepadTranslationHelper.Initialize(");
			this.Translator.TranslateExpression(output, device);
			output.Add(")");
		}

		public override void TranslatePoll(List<string> output)
		{
			output.Add("GamepadTranslationHelper.Poll()");
		}
	}
}
