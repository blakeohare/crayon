using System;
using System.Linq;
using System.Collections.Generic;
using Interpreter.Structs;
using Interpreter.Vm;

namespace Interpreter.Libraries.Game
{
    public static class AudioHelper
    {
        public static SdlDotNet.Audio.Sound GetSoundInstance(string path)
        {
            IList<byte> soundBytesList = ResourceReader.ReadSoundResource(path);
            if (soundBytesList == null) return null;
            byte[] soundBytes = soundBytesList.ToArray();
            return SdlDotNet.Audio.Mixer.Sound(soundBytes);
        }

        /*
		public static void AudioPlay(object nativeSound)
		{
			SdlDotNet.Audio.Sound sdlSound = (SdlDotNet.Audio.Sound)nativeSound;
			try
			{
				sdlSound.Play();
			}
			catch (SdlDotNet.Core.SdlException)
			{
				// No free channels or other hardware exceptions should just fail silently (no pun intended).
			}
		}//*/

        public static void AudioStop(object nativeSound)
        {
            SdlDotNet.Audio.Sound sdlSound = (SdlDotNet.Audio.Sound)nativeSound;
            try
            {
                sdlSound.Stop();
            }
            catch (SdlDotNet.Core.SdlException) { }
        }

        public static void AudioSoundStop(object rawChannel, object rawResource, int resourceId, bool isActivelyPlaying, bool isHardStop)
        {
            SdlDotNet.Audio.Channel channel = (SdlDotNet.Audio.Channel)rawChannel;
            SdlDotNet.Audio.Sound resource = (SdlDotNet.Audio.Sound)rawResource;

            if (isHardStop)
            {
                channel.Stop();
            }
            else if (isActivelyPlaying)
            {
                channel.Pause();
            }
        }

        public static int AudioSoundGetState(object rawChannel, object rawResource, int resourceId)
        {
            SdlDotNet.Audio.Channel channel = (SdlDotNet.Audio.Channel)rawChannel;
            if (channel.IsPlaying())
            {
                if (channel.IsPaused())
                {
                    return 2;
                }
                return 1;
            }
            return 3;
        }

        public static void AudioSoundResume(object rawChannel, object rawResource, double volume, double pan)
        {
            SdlDotNet.Audio.Channel channel = (SdlDotNet.Audio.Channel)rawChannel;
            if (channel.IsPaused())
            {
                channel.Resume();
                AudioApplyVolumeAndPan(channel, volume, pan);
            }
        }

        public static object AudioSoundPlay(object rawRes, double volume, double pan)
        {
            SdlDotNet.Audio.Sound sfx = (SdlDotNet.Audio.Sound)rawRes;
            try
            {
                SdlDotNet.Audio.Channel channel = sfx.Play();
                AudioApplyVolumeAndPan(channel, volume, pan);
                return channel;
            }
            catch (SdlDotNet.Core.SdlException)
            {
                return null;
            }
        }

        private static void AudioApplyVolumeAndPan(SdlDotNet.Audio.Channel channel, double volume, double pan)
        {
            channel.Volume = (int)(255 * volume);
            // TODO: apply pan using channel.SetPanning(left, right);
        }

        public static bool MusicSetVolume(double ratio)
        {
            MusicInstance.SetAmbientVolume(ratio);
            return true;
        }

        public static void AudioMusicPlay(object nativeMusicObject, bool loop)
        {
            ((MusicInstance)nativeMusicObject).Play(loop);
        }

        public static object MusicLoadResource(string path)
        {
            byte[] musicData = ResourceReader.ReadSoundResource(path);
            if (musicData == null) return null;
            return new MusicInstance(musicData);
        }

        public static bool AudioMusicIsPlaying()
        {
            return MusicInstance.IsAudioPlaying;
        }
    }
}
