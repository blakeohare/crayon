using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crayon.ParseTree;

namespace Crayon.Translator.CSharp
{
	class CSharpXamarinAndroidSystemFunctionTranslator : CSharpSystemFunctionTranslator
    {
        protected override void TranslateReadLocalSoundResource(List<string> output, Expression filePath)
        {
            output.Add("CsxaAudioHelper.GetSoundInstance(");
            this.Translator.TranslateExpression(output, filePath);
            output.Add(")");
        }

        protected override void TranslateIsWindowsProgram(List<string> output)
        {
            output.Add("TranslationHelper.AlwaysFalse()");
        }

        protected override void TranslateImageImagetteFlushToNativeBitmap(List<string> output, Expression imagette)
        {
            output.Add("CsxaTranslationHelper.ImagetteFlushToNativeBitmap(");
            this.Translator.TranslateExpression(output, imagette);
            output.Add(")");
        }

        protected override void TranslateHttpRequest(List<string> output, Expression httpRequest, Expression method, Expression url, Expression body, Expression userAgent, Expression contentType, Expression contentLength, Expression headerNameList, Expression headerValueList)
        {
            output.Add("CsxaTranslationHelper.MakeHttpRequestWithHandler(");
            this.Translator.TranslateExpression(output, httpRequest);
            output.Add(", ");
            this.Translator.TranslateExpression(output, method);
            output.Add(", ");
            this.Translator.TranslateExpression(output, url);
            output.Add(", ");
            this.Translator.TranslateExpression(output, body);
            output.Add(", ");
            this.Translator.TranslateExpression(output, userAgent);
            output.Add(", ");
            this.Translator.TranslateExpression(output, contentType);
            output.Add(", ");
            this.Translator.TranslateExpression(output, contentLength);
            output.Add(", ");
            this.Translator.TranslateExpression(output, headerNameList);
            output.Add(", ");
            this.Translator.TranslateExpression(output, headerValueList);
            output.Add(")");
        }

        protected override void TranslateAppDataRoot(List<string> output)
        {
            output.Add("\".\"");
        }

        protected override void TranslateInitializeGameWithFps(List<string> output, Expression fps)
        {
            output.Add("CsxaTranslationHelper.InitializeGameWithFps(");
            this.Translator.TranslateExpression(output, fps);
            output.Add(")");
        }

        protected override void TranslateInitializeScreen(List<string> output, Expression gameWidth, Expression gameHeight, Expression screenWidth, Expression screenHeight)
        {
            output.Add("CsxaTranslationHelper.InitializeScreen(");
            this.Translator.TranslateExpression(output, gameWidth);
            output.Add(", ");
            this.Translator.TranslateExpression(output, gameHeight);
            if (!(screenWidth is NullConstant))
            {
                output.Add(", ");
                this.Translator.TranslateExpression(output, screenWidth);
                output.Add(", ");
                this.Translator.TranslateExpression(output, screenHeight);
            }
            output.Add(")");
        }

        protected override void TranslateDownloadImage(List<string> output, Expression key, Expression path)
        {
            output.Add("CsxaTranslationHelper.DownloadImage(");
            this.Translator.TranslateExpression(output, key);
            output.Add(", ");
            this.Translator.TranslateExpression(output, path);
            output.Add(")");
        }

        protected override void TranslateGetEventsRawList(List<string> output)
        {
            output.Add("CsxaTranslationHelper.GetEvents()");
        }

        protected override void TranslateAudioMusicIsPlaying(List<string> output)
		{
			output.Add("CsxaAudioHelper.AudioIsMusicPlaying()");
		}

        protected override void TranslateImageNativeBitmapHeight(List<string> output, Expression bitmap)
        {
            output.Add("((Android.Graphics.Bitmap)");
            this.Translator.TranslateExpression(output, bitmap);
            output.Add(").Height");
        }

        protected override void TranslateImageNativeBitmapWidth(List<string> output, Expression bitmap)
        {
            output.Add("((Android.Graphics.Bitmap)");
            this.Translator.TranslateExpression(output, bitmap);
            output.Add(").Width");
        }

        protected override void TranslateAudioMusicPlayFile(List<string> output, Expression nativeResource, Expression path, Expression isLoop)
        {
            output.Add("CsxaAudioHelper.AudioMusicPlayResource(");
            this.Translator.TranslateExpression(output, nativeResource);
            output.Add(", ");
            this.Translator.TranslateExpression(output, isLoop);
            output.Add(")");
        }

		protected override void TranslateAudioMusicPlayResource(List<string> output, Expression nativeResource, Expression path, Expression isLoop)
        {
            output.Add("CsxaAudioHelper.AudioMusicPlayResource(");
            this.Translator.TranslateExpression(output, nativeResource);
            output.Add(", ");
            this.Translator.TranslateExpression(output, isLoop);
            output.Add(")");
        }

		protected override void TranslateAudioMusicVerifyFileExists(List<string> output, Expression path)
        {
            output.Add("CsxaAudioHelper.AudioMusicVerifyFileExists(");
            this.Translator.TranslateExpression(output, path);
            output.Add(")");
        }

		protected override void TranslateAudioSoundGetState(List<string> output, Expression channel, Expression resource, Expression resourceId)
        {
            output.Add("CsxaAudioHelper.AudioSoundGetState(");
            this.Translator.TranslateExpression(output, channel);
            output.Add(", ");
            this.Translator.TranslateExpression(output, resource);
            output.Add(", ");
            this.Translator.TranslateExpression(output, resourceId);
            output.Add(")");
        }

		protected override void TranslateAudioSoundPlay(List<string> output, Expression resource, Expression volume, Expression pan)
        {
            output.Add("CsxaAudioHelper.AudioSoundPlay(");
            this.Translator.TranslateExpression(output, resource);
            output.Add(", ");
            this.Translator.TranslateExpression(output, volume);
            output.Add(", ");
            this.Translator.TranslateExpression(output, pan);
            output.Add(")");
        }

		protected override void TranslateAudioSoundResume(List<string> output, Expression channel, Expression resource, Expression volumeRatio, Expression panRatio)
        {
            output.Add("CsxaAudioHelper.AudioSoundResume(");
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
            output.Add("CsxaAudioHelper.AudioSoundStop(");
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

		protected override void TranslateAudioStop(List<string> output, Expression soundInstance)
        {
            output.Add("CsxaAudioHelper.AudioStop(");
            this.Translator.TranslateExpression(output, soundInstance);
            output.Add(")");
        }

		protected override void TranslateBlitImage(List<string> output, Expression image, Expression x, Expression y)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateBlitImageAlpha(List<string> output, Expression image, Expression x, Expression y, Expression alpha)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateBlitImagePartial(List<string> output, Expression image, Expression targetX, Expression targetY, Expression targetWidth, Expression targetHeight, Expression sourceX, Expression sourceY, Expression sourceWidth, Expression sourceHeight)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateBlitImageRotated(List<string> output, Expression image, Expression centerX, Expression centerY, Expression angle)
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

		protected override void TranslateImageCreateFlippedCopyOfNativeBitmap(List<string> output, Expression image, Expression flipX, Expression flipY)
        {
            // Not used in OpenGL-based platforms.
            throw new NotImplementedException();
		}

		protected override void TranslateImageScaleNativeResource(List<string> output, Expression bitmap, Expression width, Expression height)
        {
            // Not used in OpenGL-based platforms.
            throw new NotImplementedException();
		}

		protected override void TranslateMusicLoadFromResource(List<string> output, Expression filename)
        {
            output.Add("CsxaAudioHelper.MusicLoadFromResource(");
            this.Translator.TranslateExpression(output, filename);
            output.Add(")");
        }

		protected override void TranslateMusicPause(List<string> output)
        {
            output.Add("CsxaAudioHelper.MusicPause()");
        }

		protected override void TranslateMusicPlayNow(List<string> output, Expression musicNativeObject, Expression musicRealPath, Expression isLooping)
        {
            output.Add("CsxaAudioHelper.MusicPlayNow(");
            this.Translator.TranslateExpression(output, musicNativeObject);
            output.Add(", ");
            this.Translator.TranslateExpression(output, isLooping);
            output.Add(")");
        }

		protected override void TranslateMusicSetVolume(List<string> output, Expression ratio)
		{
            output.Add("CsxaAudioHelper.MusicSetVolume(");
            this.Translator.TranslateExpression(output, ratio);
            output.Add(")");
		}

        protected override void TranslateSetTitle(List<string> output, Expression title)
        {
            output.Add("CsxaTranslationHelper.SetTitle(");
            this.Translator.TranslateExpression(output, title);
            output.Add(")");
        }

        protected override void TranslateIoCreateDirectory(List<string> output, Expression path)
        {
            output.Add("CsxaTranslationHelper.IoCreateDirectory(");
            this.Translator.TranslateExpression(output, path);
            output.Add(")");
        }

        protected override void TranslateIoCurrentDirectory(List<string> output)
        {
            output.Add("CsxaTranslationHelper.IoCurrentDirectory()");
        }

        protected override void TranslateIoDeleteDirectory(List<string> output, Expression path, Expression isRecursive)
        {
            output.Add("CsxaTranslationHelper.IoDeleteDirectory(");
            this.Translator.TranslateExpression(output, path);
            output.Add(", ");
            this.Translator.TranslateExpression(output, isRecursive);
            output.Add(")");
        }

        protected override void TranslateIoDeleteFile(List<string> output, Expression path, Expression isUserData)
        {
            output.Add("CsxaTranslationHelper.IoDeleteFile(");
            this.Translator.TranslateExpression(output, path);
            output.Add(", ");
            this.Translator.TranslateExpression(output, isUserData);
            output.Add(")");
        }

        protected override void TranslateIoDoesPathExist(List<string> output, Expression canonicalizedPath, Expression directoriesOnly, Expression performCaseCheck, Expression isUserData)
        {
            output.Add("CsxaTranslationHelper.IoPathExists(");
            this.Translator.TranslateExpression(output, canonicalizedPath);
            output.Add(", ");
            this.Translator.TranslateExpression(output, directoriesOnly);
            output.Add(", ");
            this.Translator.TranslateExpression(output, performCaseCheck);
            output.Add(", ");
            this.Translator.TranslateExpression(output, isUserData);
            output.Add(")");
        }

        protected override void TranslateIoFileReadText(List<string> output, Expression path, Expression isUserData)
        {
            output.Add("CsxaTranslationHelper.IoFileReadText(");
            this.Translator.TranslateExpression(output, path);
            output.Add(", ");
            this.Translator.TranslateExpression(output, isUserData);
            output.Add(")");
        }

        protected override void TranslateIoFilesInDirectory(List<string> output, Expression verifiedCanonicalizedPath, Expression isUserData)
        {
            output.Add("CsxaTranslationHelper.IoFilesInDirectory(");
            this.Translator.TranslateExpression(output, verifiedCanonicalizedPath);
            output.Add(", ");
            this.Translator.TranslateExpression(output, isUserData);
            output.Add(")");
        }

        protected override void TranslateIoFileWriteText(List<string> output, Expression path, Expression content, Expression isUserData)
        {
            output.Add("CsxaTranslationHelper.IoFileWriteText(");
            this.Translator.TranslateExpression(output, path);
            output.Add(", ");
            this.Translator.TranslateExpression(output, content);
            output.Add(", ");
            this.Translator.TranslateExpression(output, isUserData);
            output.Add(")");
        }
    }
}
