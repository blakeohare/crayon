using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crayon.ParseTree;

namespace Crayon.Translator.CSharp
{
	class CSharpWindowsPhoneSystemFunctionTranslator : CSharpSystemFunctionTranslator
	{
		protected override void TranslateBlitImage(List<string> output, Expression image, Expression x, Expression y)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateBlitImagePartial(List<string> output, Expression image, Expression targetX, Expression targetY, Expression targetWidth, Expression targetHeight, Expression sourceX, Expression sourceY, Expression sourceWidth, Expression sourceHeight)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDrawEllipse(List<string> output, Expression left, Expression top, Expression width, Expression height, Expression red, Expression green, Expression blue, Expression alpha)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDrawLine(List<string> output, Expression ax, Expression ay, Expression bx, Expression by, Expression lineWidth, Expression red, Expression green, Expression blue, Expression alpha)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDrawRectangle(List<string> output, Expression left, Expression top, Expression width, Expression height, Expression red, Expression green, Expression blue, Expression alpha)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDrawTriangle(List<string> output, Expression ax, Expression ay, Expression bx, Expression by, Expression cx, Expression cy, Expression red, Expression green, Expression blue, Expression alpha)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateFillScreen(List<string> output, Expression red, Expression green, Expression blue)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateGamepadEnableDevice(List<string> output, Expression device)
		{
			throw new InvalidOperationException("Gamepad not supported.");
		}

		protected override void TranslateGamepadGetAxisCount(List<string> output, Expression device)
		{
			throw new InvalidOperationException("Gamepad not supported.");
		}

		protected override void TranslateGamepadGetAxisValue(List<string> output, Expression device, Expression axisIndex)
		{
			throw new InvalidOperationException("Gamepad not supported.");
		}

		protected override void TranslateGamepadGetButtonCount(List<string> output, Expression device)
		{
			throw new InvalidOperationException("Gamepad not supported.");
		}

		protected override void TranslateGamepadGetDeviceCount(List<string> output)
		{
			throw new InvalidOperationException("Gamepad not supported.");
		}

		protected override void TranslateGamepadGetDeviceName(List<string> output, Expression device)
		{
			throw new InvalidOperationException("Gamepad not supported.");
		}

		protected override void TranslateGamepadGetHatCount(List<string> output, Expression device)
		{
			throw new InvalidOperationException("Gamepad not supported.");
		}

		protected override void TranslateGamepadGetRawDevice(List<string> output, Expression index)
		{
			throw new InvalidOperationException("Gamepad not supported.");
		}

		protected override void TranslateGamepadIsButtonPressed(List<string> output, Expression device, Expression buttonIndex)
		{
			throw new InvalidOperationException("Gamepad not supported.");
		}

		protected override void TranslateImageScaleNativeResource(List<string> output, Expression bitmap, Expression width, Expression height)
		{
			throw new InvalidOperationException();
		}

		protected override void TranslateMusicLoadFromResource(List<string> output, Expression filename, Expression intOutStatus)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateMusicPlayNow(List<string> output, Expression musicNativeObject, Expression musicRealPath, Expression isLooping)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateMusicSetVolume(List<string> output, Expression musicNativeObject, Expression ratio)
		{
			throw new NotImplementedException();
		}
	}
}
