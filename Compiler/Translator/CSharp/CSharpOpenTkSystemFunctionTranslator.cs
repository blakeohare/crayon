using System;
using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon.Translator.CSharp
{
    class CSharpOpenTkSystemFunctionTranslator : CSharpSystemFunctionTranslator
    {
        protected override void TranslateResourceGetManifest(List<string> output)
        {
            output.Add("ResourceReader.GetResourceManifest()");
        }

        protected override void TranslateReadLocalSoundResource(List<string> output, Expression filePath)
        {
            output.Add("OpenTkTranslationHelper.GetSoundInstance(\"Resources/Audio/\" + ");
            this.Translator.TranslateExpression(output, filePath);
            output.Add(")");
        }

        protected override void TranslateImageImagetteFlushToNativeBitmap(List<string> output, Expression imagette)
        {
            output.Add("OpenTkTranslationHelper.ImagetteFlushToNativeBitmap(");
            this.Translator.TranslateExpression(output, imagette);
            output.Add(")");
        }

        protected override void TranslateHttpRequest(List<string> output, Expression httpRequest, Expression method, Expression url, Expression body, Expression userAgent, Expression contentType, Expression contentLength, Expression headerNameList, Expression headerValueList)
        {
            output.Add("OpenTkTranslationHelper.MakeHttpRequestWithHandler(");
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
            output.Add("OpenTkTranslationHelper.AppDataRoot");
        }

        protected override void TranslateDownloadImage(List<string> output, Expression key, Expression path)
        {
            output.Add("OpenTkTranslationHelper.DownloadImageFromInternetTubes(");
            this.Translator.TranslateExpression(output, key);
            output.Add(", ");
            this.Translator.TranslateExpression(output, path);
            output.Add(")");
        }

        protected override void TranslateAudioMusicIsPlaying(List<string> output)
        {
            output.Add("OpenTkTranslationHelper.AudioMusicIsPlaying()");
        }

        protected override void TranslateAudioMusicPlayFile(List<string> output, Expression nativeResource, Expression path, Expression isLoop)
        {
            output.Add("OpenTkTranslationHelper.AudioMusicPlay(");
            this.Translator.TranslateExpression(output, nativeResource);
            output.Add(", ");
            this.Translator.TranslateExpression(output, isLoop);
            output.Add(")");
        }

        protected override void TranslateAudioMusicPlayResource(List<string> output, Expression nativeResource, Expression path, Expression isLoop)
        {
            // same as playing a file directly since OpenTK deals directly with native loaded resources instead of files.
            output.Add("OpenTkTranslationHelper.AudioMusicPlay(");
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
            output.Add("OpenTkTranslationHelper.AudioSoundGetState(");
            this.Translator.TranslateExpression(output, channel);
            output.Add(", ");
            this.Translator.TranslateExpression(output, resource);
            output.Add(", ");
            this.Translator.TranslateExpression(output, resourceId);
            output.Add(")");
        }

        protected override void TranslateAudioSoundPlay(List<string> output, Expression resource, Expression volume, Expression pan)
        {
            output.Add("OpenTkTranslationHelper.AudioSoundPlay(");
            this.Translator.TranslateExpression(output, resource);
            output.Add(", ");
            this.Translator.TranslateExpression(output, volume);
            output.Add(", ");
            this.Translator.TranslateExpression(output, pan);
            output.Add(")");
        }

        protected override void TranslateAudioSoundResume(List<string> output, Expression channel, Expression resource, Expression volumeRatio, Expression panRatio)
        {
            output.Add("OpenTkTranslationHelper.AudioSoundResume(");
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
            output.Add("OpenTkTranslationHelper.AudioSoundStop(");
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
        
        protected override void TranslateGetEventsRawList(List<string> output)
        {
            output.Add("OpenTkTranslationHelper.GetEvents()");
        }

        protected override void TranslateImageCreateFlippedCopyOfNativeBitmap(List<string> output, Expression image, Expression flipX, Expression flipY)
        {
            // This is not used in the OpenTK platform because flipping is simply swapping texture mapping coordinates.
            throw new NotImplementedException();
        }

        protected override void TranslateImageNativeBitmapHeight(List<string> output, Expression bitmap)
        {
            output.Add("((System.Drawing.Bitmap)");
            this.Translator.TranslateExpression(output, bitmap);
            output.Add(").Height");
        }

        protected override void TranslateImageNativeBitmapWidth(List<string> output, Expression bitmap)
        {
            output.Add("((System.Drawing.Bitmap)");
            this.Translator.TranslateExpression(output, bitmap);
            output.Add(").Width");
        }

        protected override void TranslateImageScaleNativeResource(List<string> output, Expression bitmap, Expression width, Expression height)
        {
            throw new InvalidOperationException();
        }

        protected override void TranslateMusicLoadFromResource(List<string> output, Expression filename)
        {
            output.Add("OpenTkTranslationHelper.MusicLoadResource(\"Resources/Audio/\" + ");
            this.Translator.TranslateExpression(output, filename);
            output.Add(")");
        }

        protected override void TranslateMusicPause(List<string> output)
        {
            output.Add("OpenTkTranslationHelper.MusicPause()");
        }

        protected override void TranslateMusicPlayNow(List<string> output, Expression musicNativeObject, Expression musicRealPath, Expression isLooping)
        {
            output.Add("OpenTkTranslationHelper.MusicPlayNow(");
            this.Translator.TranslateExpression(output, musicNativeObject);
            output.Add(", ");
            this.Translator.TranslateExpression(output, isLooping);
            output.Add(")");
        }

        protected override void TranslateMusicSetVolume(List<string> output, Expression ratio)
        {
            output.Add("OpenTkTranslationHelper.MusicSetVolume(");
            this.Translator.TranslateExpression(output, ratio);
            output.Add(")");
        }

        protected override void TranslateAudioPlay(List<string> output, Expression soundInstance)
        {
            output.Add("OpenTkTranslationHelper.AudioPlay(");
            this.Translator.TranslateExpression(output, soundInstance);
            output.Add(")");
        }

        protected override void TranslateAudioStop(List<string> output, Expression soundInstance)
        {
            output.Add("OpenTkTranslationHelper.AudioStop(");
            this.Translator.TranslateExpression(output, soundInstance);
            output.Add(")");
        }

        protected override void TranslateSetTitle(List<string> output, Expression title)
        {
            output.Add("GameWindow.Instance.SetTitle(");
            this.Translator.TranslateExpression(output, title);
            output.Add(")");
        }

        protected override void TranslateInitializeGameWithFps(List<string> output, Expression fps)
        {
            output.Add("GameWindow.FPS = ");
            this.Translator.TranslateExpression(output, fps);
        }

        protected override void TranslateInitializeScreen(List<string> output, Expression gameWidth, Expression gameHeight, Expression screenWidth, Expression screenHeight)
        {
            output.Add("GameWindow.InitializeScreen(");
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

        protected override void TranslateIoCreateDirectory(List<string> output, Expression path)
        {
            output.Add("OpenTkTranslationHelper.CreateDirectory(");
            this.Translator.TranslateExpression(output, path);
            output.Add(")");
        }

        protected override void TranslateIoCurrentDirectory(List<string> output)
        {
            output.Add("System.IO.Directory.GetCurrentDirectory()");
        }

        protected override void TranslateIoDeleteDirectory(List<string> output, Expression path, Expression isRecursive)
        {
            output.Add("OpenTkTranslationHelper.DeleteDirectory(");
            this.Translator.TranslateExpression(output, path);
            output.Add(", ");
            this.Translator.TranslateExpression(output, isRecursive);
            output.Add(")");
        }

        protected override void TranslateIoDeleteFile(List<string> output, Expression path, Expression isUserData)
        {
            output.Add("OpenTkTranslationHelper.DeleteFile(");
            this.Translator.TranslateExpression(output, path);
            output.Add(")");
        }

        protected override void TranslateIoDoesPathExist(List<string> output, Expression canonicalizedPath, Expression directoriesOnly, Expression performCaseCheck, Expression isUserData)
        {
            output.Add("OpenTkTranslationHelper.DoesPathExist(");
            this.Translator.TranslateExpression(output, canonicalizedPath);
            output.Add(", ");
            this.Translator.TranslateExpression(output, directoriesOnly);
            output.Add(", ");
            this.Translator.TranslateExpression(output, performCaseCheck);
            output.Add(")");
        }

        protected override void TranslateIoFileReadText(List<string> output, Expression path, Expression isUserData)
        {
            output.Add("OpenTkTranslationHelper.ReadFile(");
            this.Translator.TranslateExpression(output, path);
            output.Add(")");
        }

        protected override void TranslateIoFilesInDirectory(List<string> output, Expression verifiedCanonicalizedPath, Expression isUserData)
        {
            output.Add("OpenTkTranslationHelper.FilesInDirectory(");
            this.Translator.TranslateExpression(output, verifiedCanonicalizedPath);
            output.Add(")");
        }

        protected override void TranslateIoFileWriteText(List<string> output, Expression path, Expression content, Expression isUserData)
        {
            output.Add("OpenTkTranslationHelper.WriteFile(");
            this.Translator.TranslateExpression(output, path);
            output.Add(", ");
            this.Translator.TranslateExpression(output, content);
            output.Add(")");
        }

        protected override void TranslateGlMaxTextureSize(List<string> output)
        {
            output.Add("GlUtil.MaxTextureSize");
        }
    }
}
