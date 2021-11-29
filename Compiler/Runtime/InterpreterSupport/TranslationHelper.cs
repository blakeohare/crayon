using System;
using System.Runtime.InteropServices;

namespace Interpreter.Vm
{
    public static class TranslationHelper
    {
        internal static EventLoop GetEventLoop(Interpreter.Structs.VmContext vm)
        {
            return (EventLoop)CrayonWrapper.vmGetEventLoopObj(vm);
        }

        public static string ResourceManifest { get; set; }
        public static string ByteCode { get; set; }

        public static readonly bool IsWindows =
            Environment.OSVersion.Platform == PlatformID.Win32NT ||
            Environment.OSVersion.Platform == PlatformID.Win32S ||
            Environment.OSVersion.Platform == PlatformID.Win32Windows ||
            Environment.OSVersion.Platform == PlatformID.WinCE;

        private static string environmentDescriptor = null;
        public static string EnvironmentDescriptor
        {
            get
            {
                if (environmentDescriptor == null)
                {
                    if (Environment.OSVersion.Platform == PlatformID.Unix)
                    {
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                        {
                            environmentDescriptor = "mac|csharp";
                        }
                        else
                        {
                            environmentDescriptor = "linux|csharp";
                        }
                    }
                    else
                    {
                        environmentDescriptor = "windows|csharp";
                    }
                }
                return environmentDescriptor;
            }
        }

        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.Winapi)]
        private static extern void GetSystemTimePreciseAsFileTime(out long filetime);
        private static DateTime Epoch = new DateTime(1970, 1, 1); // TODO: switch to UnixEpoch once this is migrated to .NET Core
        public static double GetCurrentTime()
        {
            if (IsWindows)
            {
                long filetime;
                GetSystemTimePreciseAsFileTime(out filetime);
                DateTime now = DateTime.FromFileTimeUtc(filetime);
                return now.Subtract(Epoch).Ticks / (double)TimeSpan.TicksPerSecond;
            }
            return DateTime.UtcNow.Subtract(Epoch).TotalMilliseconds / 1000.0;
        }

        public static void Noop() { }
        public static bool AlwaysTrue() { return true; }
        public static bool AlwaysFalse() { return false; }
    }
}
