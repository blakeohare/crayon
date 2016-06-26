using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crayon.ParseTree;

namespace Crayon.Translator.Java
{
    internal class JavaAndroidSystemFunctionTranslator : JavaSystemFunctionTranslator
    {
        protected override void TranslateResourceGetManifest(List<string> output)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateAudioMusicIsPlaying(List<string> output)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateAudioMusicPlayFile(List<string> output, Expression nativeResource, Expression path, Expression isLoop)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateAudioMusicPlayResource(List<string> output, Expression nativeResource, Expression path, Expression isLoop)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateAudioMusicVerifyFileExists(List<string> output, Expression path)
        {
            // cannot play files from disk on Android.
            output.Add("false");
        }

        protected override void TranslateAudioSoundGetState(List<string> output, Expression channel, Expression resource, Expression resourceId)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateAudioSoundPlay(List<string> output, Expression resource, Expression volume, Expression pan)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateAudioSoundResume(List<string> output, Expression channel, Expression resource, Expression volumeRatio, Expression panRatio)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateAudioSoundStop(List<string> output, Expression channel, Expression resource, Expression resourceId, Expression isActivelyPlaying, Expression isHardStop)
        {
            throw new NotImplementedException();
        }
        
        protected override void TranslateHttpRequest(List<string> output, Expression httpRequest, Expression method, Expression url, Expression body, Expression userAgent, Expression contentType, Expression contentLength, Expression headerNameList, Expression headerValueList)
        {
            output.Add("AndroidTranslationHelper.makeHttpRequest(");
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

        protected override void TranslateIsWindowsProgram(List<string> output)
        {
            output.Add("AndroidTranslationHelper.isWindows()");
        }
        
        protected override void TranslateImageCreateFlippedCopyOfNativeBitmap(List<string> output, Expression image, Expression flipX, Expression flipY)
        {
            output.Add("AndroidTranslationHelper.flipImage(");
            this.Translator.TranslateExpression(output, image);
            output.Add(", ");
            this.Translator.TranslateExpression(output, flipX);
            output.Add(", ");
            this.Translator.TranslateExpression(output, flipY);
            output.Add(")");
        }
        
        protected override void TranslateInitializeGameWithFps(List<string> output, Expression fps)
        {
            output.Add("AndroidTranslationHelper.initializeGame(");
            this.Translator.TranslateExpression(output, fps);
            output.Add(")");
        }

        protected override void TranslateGetRawByteCodeString(List<string> output)
        {
            output.Add("AndroidTranslationHelper.getRawByteCodeString()");
        }

        protected override void TranslateAppDataRoot(List<string> output)
        {
            output.Add("AndroidTranslationHelper.getAppDataRoot()");
        }

        protected override void TranslateGetEventsRawList(List<string> output)
        {
            output.Add("AndroidTranslationHelper.getEventsRawList()");
        }

        protected override void TranslateImageNativeBitmapHeight(List<string> output, Expression bitmap)
        {
            output.Add("((android.graphics.Bitmap)");
            this.Translator.TranslateExpression(output, bitmap);
            output.Add(").getHeight()");
        }

        protected override void TranslateImageNativeBitmapWidth(List<string> output, Expression bitmap)
        {
            output.Add("((android.graphics.Bitmap)");
            this.Translator.TranslateExpression(output, bitmap);
            output.Add(").getWidth()");
        }

        protected override void TranslateImageScaleNativeResource(List<string> output, Expression bitmap, Expression width, Expression height)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateInitializeScreen(List<string> output, Expression gameWidth, Expression gameHeight, Expression screenWidth, Expression screenHeight)
        {
            output.Add("AndroidTranslationHelper.initializeScreen(");
            this.Translator.TranslateExpression(output, gameWidth);
            output.Add(", ");
            this.Translator.TranslateExpression(output, gameHeight);
            output.Add(", ");
            if (screenWidth is NullConstant)
            {
                // TODO: this is silly and now duplicated in JavaAwt. refactor this.
                this.Translator.TranslateExpression(output, gameWidth);
                output.Add(", ");
                this.Translator.TranslateExpression(output, gameHeight);
            }
            else
            {
                this.Translator.TranslateExpression(output, screenWidth);
                output.Add(", ");
                this.Translator.TranslateExpression(output, screenHeight);
            }
            output.Add(")");
        }

        protected override void TranslateIoCreateDirectory(List<string> output, Expression path)
        {
            output.Add("AndroidTranslationHelper.ioCreateDirectory(");
            this.Translator.TranslateExpression(output, path);
            output.Add(")");
        }

        protected override void TranslateIoCurrentDirectory(List<string> output)
        {
            output.Add("AndroidTranslationHelper.ioCurrentDirectory()");
        }

        protected override void TranslateIoDeleteDirectory(List<string> output, Expression path, Expression isRecursive)
        {
            output.Add("AndroidTranslationHelper.ioDeleteDirectory(");
            this.Translator.TranslateExpression(output, path);
            output.Add(", ");
            this.Translator.TranslateExpression(output, isRecursive);
            output.Add(")");
        }

        protected override void TranslateIoDeleteFile(List<string> output, Expression path, Expression isUserData)
        {
            output.Add("AndroidTranslationHelper.ioDeleteFile(");
            this.Translator.TranslateExpression(output, path);
            output.Add(", ");
            this.Translator.TranslateExpression(output, isUserData);
            output.Add(")");
        }

        protected override void TranslateIoDoesPathExist(List<string> output, Expression canonicalizedPath, Expression directoriesOnly, Expression performCaseCheck, Expression isUserData)
        {
            output.Add("AndroidTranslationHelper.ioDoesPathExist(");
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
            output.Add("AndroidTranslationHelper.ioFileReadText(");
            this.Translator.TranslateExpression(output, path);
            output.Add(", ");
            this.Translator.TranslateExpression(output, isUserData);
            output.Add(")");
        }

        protected override void TranslateIoFilesInDirectory(List<string> output, Expression verifiedCanonicalizedPath, Expression isUserData)
        {
            output.Add("AndroidTranslationHelper.ioFilesInDirectory(");
            this.Translator.TranslateExpression(output, verifiedCanonicalizedPath);
            output.Add(", ");
            this.Translator.TranslateExpression(output, isUserData);
            output.Add(")");
        }

        protected override void TranslateIoFileWriteText(List<string> output, Expression path, Expression content, Expression isUserData)
        {
            output.Add("AndroidTranslationHelper.ioFileWriteText(");
            this.Translator.TranslateExpression(output, path);
            output.Add(", ");
            this.Translator.TranslateExpression(output, content);
            output.Add(", ");
            this.Translator.TranslateExpression(output, isUserData);
            output.Add(")");
        }

        protected override void TranslateMusicLoadFromResource(List<string> output, Expression filename)
        {
            output.Add("AndroidTranslationHelper.loadMusicFromResource(");
            this.Translator.TranslateExpression(output, filename);
            output.Add(")");
        }

        protected override void TranslateMusicPause(List<string> output)
        {
            output.Add("AndroidTranslationHelper.pauseMusic()");
        }

        protected override void TranslateMusicPlayNow(List<string> output, Expression musicNativeObject, Expression musicRealPath, Expression isLooping)
        {
            output.Add("AndroidTranslationHelper.playMusicNow(");
            this.Translator.TranslateExpression(output, musicNativeObject);
            output.Add(", ");
            this.Translator.TranslateExpression(output, isLooping);
            output.Add(")");
        }

        protected override void TranslateMusicSetVolume(List<string> output, Expression ratio)
        {
            output.Add("AndroidTranslationHelper.setMusicVolume(");
            this.Translator.TranslateExpression(output, ratio);
            output.Add(")");
        }

        protected override void TranslateReadLocalImageResource(List<string> output, Expression filePath)
        {
            output.Add("AndroidTranslationHelper.readLocalImageResource(");
            this.Translator.TranslateExpression(output, filePath);
            output.Add(")");
        }

        protected override void TranslateReadLocalSoundResource(List<string> output, Expression filePath)
        {
            output.Add("AndroidTranslationHelper.readLocalSoundResource(");
            this.Translator.TranslateExpression(output, filePath);
            output.Add(")");
        }

        protected override void TranslateResourceReadText(List<string> output, Expression path)
        {
            output.Add("AndroidTranslationHelper.resourceReadText(");
            this.Translator.TranslateExpression(output, path);
            output.Add(")");
        }

        protected override void TranslateSetTitle(List<string> output, Expression title)
        {
            output.Add("AndroidTranslationHelper.setTitle(");
            this.Translator.TranslateExpression(output, title);
            output.Add(")");
        }
        
        protected override void TranslateAudioStop(List<string> output, Expression soundInstance)
        {
            output.Add("AndroidTranslationHelper.stopSound(");
            this.Translator.TranslateExpression(output, soundInstance);
            output.Add(")");
        }

        protected override void TranslateImageImagetteFlushToNativeBitmap(List<string> output, Expression imagette)
        {
            output.Add("AndroidTranslationHelper.flushImagetteToBitmap(");
            this.Translator.TranslateExpression(output, imagette);
            output.Add(")");
        }

        protected override void TranslateImageInitiateAsyncDownloadOfResource(List<string> output, Expression path)
        {
            output.Add("AndroidTranslationHelper.imageInitializeAsyncDownloadOfResource(");
            this.Translator.TranslateExpression(output, path);
            output.Add(")");
        }

        protected override void TranslateGlMaxTextureSize(List<string> output)
        {
            throw new NotImplementedException();
        }
    }
}
