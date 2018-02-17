
namespace Interpreter.Libraries.GameGifCap
{
    public static class GameGifCapHelper
    {
        public static int SaveToDisk(string path, object[] images, int millisPerFrame)
        {
            throw new System.NotImplementedException();
        }

        public static int ScreenCap(object[] output)
        {
            if (Game.GameWindow.Instance == null)
            {
                return 1;
            }
            output[0] = Game.GameWindow.Instance.ScreenCapture();
            return 0;
        }
    }
}
