using System.Linq;

namespace Interpreter.Libraries.GameGifCap
{
    public static class GameGifCapHelper
    {
        public static int SaveToDisk(string path, object[] images, int millisPerFrame)
        {
            System.Drawing.Bitmap[] frames = images.Cast<System.Drawing.Bitmap>().ToArray();
            System.IO.FileStream stream = System.IO.File.Create(path);
            using (BumpKitGifEncoder gifEncoder = new BumpKitGifEncoder(stream, frames[0].Width, frames[0].Height))
            {
                foreach (System.Drawing.Bitmap frame in frames)
                {
                    gifEncoder.AddFrame(frame, 0, 0, System.TimeSpan.FromMilliseconds(millisPerFrame));
                }
            }
            stream.Flush();
            stream.Close();

            return 0;
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
