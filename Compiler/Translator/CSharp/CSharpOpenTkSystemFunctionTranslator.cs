using System;
using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon.Translator.CSharp
{
	class CSharpOpenTkSystemFunctionTranslator : CSharpSystemFunctionTranslator
	{
		protected override void TranslateAudioMusicIsPlaying(List<string> output)
		{
			output.Add("TranslationHelper.AudioMusicIsPlaying()");
		}

		protected override void TranslateAudioMusicPlayFile(List<string> output, Expression nativeResource, Expression path, Expression isLoop)
		{
			output.Add("TranslationHelper.AudioMusicPlay(");
			this.Translator.TranslateExpression(output, nativeResource);
			output.Add(", ");
			this.Translator.TranslateExpression(output, isLoop);
			output.Add(")");
		}

		protected override void TranslateAudioMusicPlayResource(List<string> output, Expression nativeResource, Expression path, Expression isLoop)
		{
			// same as playing a file directly since OpenTK deals directly with native loaded resources instead of files.
			output.Add("TranslationHelper.AudioMusicPlay(");
			this.Translator.TranslateExpression(output, nativeResource);
			output.Add(", ");
			this.Translator.TranslateExpression(output, isLoop);
			output.Add(")");
		}

		protected override void TranslateAudioMusicVerifyFileExists(List<string> output, Expression path)
		{
			output.Add("System.IO.File.Exists(");
			this.Translator.TranslateExpression(output, path);
			output.Add(")");
		}

		protected override void TranslateAudioSoundGetState(List<string> output, Expression channel, Expression resource, Expression resourceId)
		{
			output.Add("TranslationHelper.AudioSoundGetState(");
			this.Translator.TranslateExpression(output, channel);
			output.Add(", ");
			this.Translator.TranslateExpression(output, resource);
			output.Add(", ");
			this.Translator.TranslateExpression(output, resourceId);
			output.Add(")");
		}

		protected override void TranslateAudioSoundPlay(List<string> output, Expression resource, Expression volume, Expression pan)
		{
			output.Add("TranslationHelper.AudioSoundPlay(");
			this.Translator.TranslateExpression(output, resource);
			output.Add(", ");
			this.Translator.TranslateExpression(output, volume);
			output.Add(", ");
			this.Translator.TranslateExpression(output, pan);
			output.Add(")");
		}

		protected override void TranslateAudioSoundResume(List<string> output, Expression channel, Expression resource, Expression volumeRatio, Expression panRatio)
		{
			output.Add("TranslationHelper.AudioSoundResume(");
			this.Translator.TranslateExpression(output, channel);
			output.Add(", ");
			this.Translator.TranslateExpression(output, resource);
			output.Add(", ");
			this.Translator.TranslateExpression(output, volumeRatio);
			output.Add(", ");
			this.Translator.TranslateExpression(output, panRatio);
			output.Add(")");
		}

		protected override void TranslateAudioSoundStop(List<string> output, Expression channel, Expression resource, Expression resourceId, Expression isActivelyPlaying, Expression isHardStop)
		{
			output.Add("TranslationHelper.AudioSoundStop(");
			this.Translator.TranslateExpression(output, channel);
			output.Add(", ");
			this.Translator.TranslateExpression(output, resource);
			output.Add(", ");
			this.Translator.TranslateExpression(output, resourceId);
			output.Add(", ");
			this.Translator.TranslateExpression(output, isActivelyPlaying);
			output.Add(", ");
			this.Translator.TranslateExpression(output, isHardStop);
			output.Add(")");
		}

		protected override void TranslateBlitImage(List<string> output, Expression image, Expression x, Expression y)
		{
			throw new InvalidOperationException("Should be optimized out.");
		}

		protected override void TranslateBlitImageAlpha(List<string> output, Expression image, Expression x, Expression y, Expression alpha)
		{
			throw new InvalidOperationException("Should be optimized out.");
		}

		protected override void TranslateBlitImagePartial(List<string> output, Expression image, Expression targetX, Expression targetY, Expression targetWidth, Expression targetHeight, Expression sourceX, Expression sourceY, Expression sourceWidth, Expression sourceHeight)
		{
			throw new InvalidOperationException("Should be optimized out.");
		}

		protected override void TranslateBlitImageRotated(List<string> output, Expression image, Expression centerX, Expression centerY, Expression angle)
		{
			throw new InvalidOperationException("Should be optimized out.");
		}

		protected override void TranslateDrawEllipse(List<string> output, Expression left, Expression top, Expression width, Expression height, Expression red, Expression green, Expression blue, Expression alpha)
		{
			throw new InvalidOperationException("Should be optimized out.");
		}

		protected override void TranslateDrawLine(List<string> output, Expression ax, Expression ay, Expression bx, Expression by, Expression lineWidth, Expression red, Expression green, Expression blue, Expression alpha)
		{
			throw new InvalidOperationException("Should be optimized out.");
		}

		protected override void TranslateDrawRectangle(List<string> output, Expression left, Expression top, Expression width, Expression height, Expression red, Expression green, Expression blue, Expression alpha)
		{
			throw new InvalidOperationException("Should be optimized out.");
		}

		protected override void TranslateDrawTriangle(List<string> output, Expression ax, Expression ay, Expression bx, Expression by, Expression cx, Expression cy, Expression red, Expression green, Expression blue, Expression alpha)
		{
			throw new InvalidOperationException("Should be optimized out.");
		}

		protected override void TranslateFillScreen(List<string> output, Expression red, Expression green, Expression blue)
		{
			throw new InvalidOperationException("Should be optimized out.");
		}

		protected override void TranslateImageScaleNativeResource(List<string> output, Expression bitmap, Expression width, Expression height)
		{
			throw new InvalidOperationException();
		}

		protected override void TranslateMusicLoadFromResource(List<string> output, Expression filename)
		{
			output.Add("TranslationHelper.MusicLoadResource(");
			this.Translator.TranslateExpression(output, filename);
			output.Add(")");
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

		protected override void TranslateMusicSetVolume(List<string> output, Expression ratio)
		{
			output.Add("TranslationHelper.MusicSetVolume(");
			this.Translator.TranslateExpression(output, ratio);
			output.Add(")");
		}

		protected override void TranslateAudioPlay(List<string> output, Expression soundInstance)
		{
			output.Add("TranslationHelper.AudioPlay(");
			this.Translator.TranslateExpression(output, soundInstance);
			output.Add(")");
		}

		protected override void TranslateAudioStop(List<string> output, Expression soundInstance)
		{
			output.Add("TranslationHelper.AudioStop(");
			this.Translator.TranslateExpression(output, soundInstance);
			output.Add(")");
		}
	}
}
