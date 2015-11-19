using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon.Translator.CSharp
{
	class CSharpOpenTkSystemFunctionTranslator : CSharpSystemFunctionTranslator
	{
		protected override void TranslateBlitImage(List<string> output, Expression image, Expression x, Expression y)
		{
			throw new System.InvalidOperationException("Should be optimized out.");
		}

		protected override void TranslateBlitImageAlpha(List<string> output, Expression image, Expression x, Expression y, Expression alpha)
		{
			throw new System.InvalidOperationException("Should be optimized out.");
		}

		protected override void TranslateBlitImagePartial(List<string> output, Expression image, Expression targetX, Expression targetY, Expression targetWidth, Expression targetHeight, Expression sourceX, Expression sourceY, Expression sourceWidth, Expression sourceHeight)
		{
			throw new System.InvalidOperationException("Should be optimized out.");
		}

		protected override void TranslateDrawEllipse(List<string> output, Expression left, Expression top, Expression width, Expression height, Expression red, Expression green, Expression blue, Expression alpha)
		{
			throw new System.InvalidOperationException("Should be optimized out.");
		}

		protected override void TranslateDrawLine(List<string> output, Expression ax, Expression ay, Expression bx, Expression by, Expression lineWidth, Expression red, Expression green, Expression blue, Expression alpha)
		{
			throw new System.InvalidOperationException("Should be optimized out.");
		}

		protected override void TranslateDrawRectangle(List<string> output, Expression left, Expression top, Expression width, Expression height, Expression red, Expression green, Expression blue, Expression alpha)
		{
			throw new System.InvalidOperationException("Should be optimized out.");
		}

		protected override void TranslateDrawTriangle(List<string> output, Expression ax, Expression ay, Expression bx, Expression by, Expression cx, Expression cy, Expression red, Expression green, Expression blue, Expression alpha)
		{
			throw new System.InvalidOperationException("Should be optimized out.");
		}

		protected override void TranslateFillScreen(List<string> output, Expression red, Expression green, Expression blue)
		{
			throw new System.InvalidOperationException("Should be optimized out.");
		}

		protected override void TranslateGamepadEnableDevice(List<string> output, Expression device)
		{
			output.Add("GameWindow.Instance.GamepadEnableDevice((OpenTK.Input.JoystickDevice)");
			this.Translator.TranslateExpression(output, device);
			output.Add(")");
		}

		protected override void TranslateGamepadGetAxisCount(List<string> output, Expression device)
		{
			output.Add("GameWindow.Instance.GetGamepadAxisCount((OpenTK.Input.JoystickDevice)");
			this.Translator.TranslateExpression(output, device);
			output.Add(")");
		}

		protected override void TranslateGamepadGetAxisValue(List<string> output, Expression device, Expression axisIndex)
		{
			output.Add("GameWindow.Instance.GetGamepadAxisValue((OpenTK.Input.JoystickDevice)");
			this.Translator.TranslateExpression(output, device);
			output.Add(", ");
			this.Translator.TranslateExpression(output, axisIndex);
			output.Add(")");
		}
		
		protected override void TranslateGamepadGetButtonCount(List<string> output, Expression device)
		{
			output.Add("GameWindow.Instance.GetGamepadButtonCount((OpenTK.Input.JoystickDevice)");
			this.Translator.TranslateExpression(output, device);
			output.Add(")");
		}

		protected override void TranslateGamepadGetDeviceCount(List<string> output)
		{
			output.Add("GameWindow.Instance.GetGamepadCount()");
		}

		protected override void TranslateGamepadGetDeviceName(List<string> output, Expression device)
		{
			output.Add("GameWindow.Instance.GetGamepadDeviceName((OpenTK.Input.JoystickDevice)");
			this.Translator.TranslateExpression(output, device);
			output.Add(")");
		}

		protected override void TranslateGamepadGetRawDevice(List<string> output, Expression index)
		{
			output.Add("GameWindow.Instance.GetGamepadRawDevice(");
			this.Translator.TranslateExpression(output, index);
			output.Add(")");
		}

		protected override void TranslateGamepadGetHatCount(List<string> output, Expression device)
		{
			output.Add("GameWindow.Instance.GetGamepadHatCount((OpenTK.Input.JoystickDevice)");
			this.Translator.TranslateExpression(output, device);
			output.Add(")");
		}

		protected override void TranslateGamepadIsButtonPressed(List<string> output, Expression device, Expression buttonIndex)
		{
			output.Add("GameWindow.Instance.IsGamepadButtonPushed((OpenTK.Input.JoystickDevice)");
			this.Translator.TranslateExpression(output, device);
			output.Add(", ");
			this.Translator.TranslateExpression(output, buttonIndex);
			output.Add(")");
		}

		protected override void TranslateImageScaleNativeResource(List<string> output, Expression bitmap, Expression width, Expression height)
		{
			throw new System.InvalidOperationException();
		}

		protected override void TranslateMusicLoadFromResource(List<string> output, Expression filename, Expression intOutStatus)
		{
			output.Add("TranslationHelper.MusicLoad(");
			this.Translator.TranslateExpression(output, filename);
			output.Add(", ");
			this.Translator.TranslateExpression(output, intOutStatus);
			output.Add(", true)");
		}

		protected override void TranslateMusicPause(List<string> output)
		{
			output.Add("TranslationHelper.MusicPause()");
		}

		protected override void TranslateMusicPlayNow(List<string> output, Expression musicNativeObject, Expression musicRealPath, Expression isLooping)
		{
			output.Add("TranslationHelper.MusicPlayNow(");
			this.Translator.TranslateExpression(output, musicNativeObject);
			output.Add(", ");
			this.Translator.TranslateExpression(output, isLooping);
			output.Add(")");
		}

		protected override void TranslateMusicResume(List<string> output)
		{
			output.Add("TranslationHelper.MusicResume()");
		}

		protected override void TranslateMusicSetVolume(List<string> output, Expression musicNativeObject, Expression ratio)
		{
			output.Add("TranslationHelper.MusicSetVolume(");
			this.Translator.TranslateExpression(output, ratio);
			output.Add(")");
		}

		protected override void TranslateSfxPlay(List<string> output, Expression soundInstance)
		{
			output.Add("TranslationHelper.SfxPlay(");
			this.Translator.TranslateExpression(output, soundInstance);
			output.Add(")");
		}
	}
}
