namespace Interpreter.Libraries.GameGifCap
{
    public static class GameGifCapHelper
    {
        public static bool IsSupported()
        {
#if WINDOWS
            return true;
#else
            return false;
#endif
        }

        public static int SaveToDisk(object gifRecContextObj, string path)
        {
            return ((GifRecorderContext)gifRecContextObj).Finish(path);
        }

        public static int ScreenCap(object gifContext, object nativeWindowInstance)
        {
            GifRecorderContext grc = (GifRecorderContext)gifContext;

            object screenImage = nativeWindowInstance.GetType()
                .GetMethod("ScreenCapture")
                .Invoke(nativeWindowInstance, new object[0]);

            grc.AddImage(screenImage);

            return 0;
        }
    }
}
