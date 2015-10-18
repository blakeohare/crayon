using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon.Translator.CSharp
{
	internal class CSharpOpenTkGamepadTranslator : AbstractGamepadTranslator
	{
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
	}
}
