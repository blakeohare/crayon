using System;

namespace Interpreter.Libraries.Game
{
    public class MusicInstance
    {
        private static bool IsAudioAllowed =
            !(System.Environment.OSVersion.Platform == PlatformID.Unix ||
            System.Environment.OSVersion.Platform == PlatformID.MacOSX);

        public object internalMusic;

        public MusicInstance(byte[] bytes)
        {
            if (IsAudioAllowed)
            {
                this.internalMusic = new SdlDotNet.Audio.Music(bytes);
            }
        }

        public void Play(bool isLooping)
        {
            if (IsAudioAllowed)
            {
                if (isLooping)
                {
                    ((SdlDotNet.Audio.Music)this.internalMusic).Play(true);
                }
                else
                {
                    ((SdlDotNet.Audio.Music)this.internalMusic).Play(1);
                }
            }
        }

        public static void SetAmbientVolume(double ratio)
        {
            if (IsAudioAllowed)
            {
                SdlDotNet.Audio.MusicPlayer.Volume = (int)(ratio * 255);
            }
        }

        public static bool IsAudioPlaying
        {
            get
            {
                return IsAudioAllowed && SdlDotNet.Audio.MusicPlayer.IsPlaying;
            }
        }
    }
}
