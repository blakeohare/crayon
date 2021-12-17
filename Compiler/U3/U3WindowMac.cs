namespace U3
{
#if MAC
    using System;
    using System.Threading.Tasks;

    internal class U3WindowMac : U3Window
    {
        internal override Task<string> CreateAndShowWindow(string title, string icon, int width, int height, bool keepAspectRatio, object[] initialData)
        {
            throw new NotImplementedException();
        }

        internal override Task SendDataBuffer(object[] buffer)
        {
            throw new NotImplementedException();
        }
    }
#endif
}
