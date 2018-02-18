namespace Interpreter.Libraries.GameGifCap
{
    public static class GameGifCapHelper
    {
        public static int SaveToDisk(object gifRecContextObj, string path)
        {
            return ((GifRecorderContext)gifRecContextObj).Finish(path);
        }

        public static int ScreenCap(object gifContext)
        {
            if (Game.GameWindow.Instance == null)
            {
                return 1;
            }

            GifRecorderContext grc = (GifRecorderContext)gifContext;
            object screenImage = Game.GameWindow.Instance.ScreenCapture();
            grc.AddImage(screenImage);

            return 0;
        }
    }
}
