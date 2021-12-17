namespace U3
{
    internal class PlatformUtil
    {
#if WINDOWS
        internal static readonly string PLATFORM = "win";
#elif MAC
        internal static readonly string PLATFORM = "mac";
#elif LINUX
        internal static readonly string PLATFORM = "linux";
#endif

        internal static U3Window CreateWindow(int id)
        {
#if WINDOWS
            return new U3WindowWindows() { ID = id };
#elif MAC
            return new U3WindowMac() { ID = id };
#elif LINUX
            throw new System.NotImplementedException();
#else
            throw new System.NotImplementedException();
#endif
        }
    }
}
