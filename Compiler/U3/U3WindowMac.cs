namespace U3
{
#if MAC
    using System;
    using System.Threading.Tasks;

    internal class U3WindowMac : U3Window
    {
        internal override Task<string> CreateAndShowWindowImpl(string title, byte[] nullableIcon, int width, int height, Func<string, string, bool> handleVmBoundMessage)
        {
            throw new NotImplementedException();
        }

        internal override Task SendJsonData(string jsonString)
        {
            throw new NotImplementedException();
        }
    }
#endif
}
