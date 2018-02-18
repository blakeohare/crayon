namespace Interpreter.Libraries.GameGifCap
{
    public class GifRecorderContext
    {
        private BumpKitGifEncoder bkGifEncoder = null;
        private int millisPerFrame;
        private int expectedWidth;
        private int expectedHeight;
        private string tempFile;
        private System.IO.FileStream tempFileStream;

        public GifRecorderContext(int millisPerFrame)
        {
            this.millisPerFrame = millisPerFrame;
            this.tempFile = System.IO.Path.GetTempFileName() + ".gif";
        }

        public void AddImage(object frameObj)
        {
            System.Drawing.Bitmap frame = (System.Drawing.Bitmap)frameObj;

            // Not sure why this is getting flipped during the encoding process, so
            // pre-flip it as a workaround.
            frame.RotateFlip(System.Drawing.RotateFlipType.RotateNoneFlipY);

            int width = frame.Width;
            int height = frame.Height;
            if (bkGifEncoder == null)
            {
                this.expectedWidth = width;
                this.expectedHeight = height;
                this.tempFileStream = System.IO.File.Create(this.tempFile);
                this.bkGifEncoder = new BumpKitGifEncoder(this.tempFileStream, width, height);
            }
            else
            {
                if (this.expectedWidth != width || this.expectedHeight != height)
                {
                    // TODO: error code
                    throw new System.Exception();
                }
            }

            this.bkGifEncoder.AddFrame(frame, 0, 0, System.TimeSpan.FromMilliseconds(this.millisPerFrame));
        }

        public int Finish(string path)
        {
            if (this.bkGifEncoder == null)
            {
                return 1;
            }

            try
            {
                this.bkGifEncoder.Dispose();
                this.bkGifEncoder = null;
                this.tempFileStream.Flush();
                this.tempFileStream.Close();
            }
            catch (System.Exception)
            {
                return 2;
            }

            try
            {
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
                System.IO.File.Move(this.tempFile, path);
            }
            catch (System.Exception)
            {
                return 3;
            }

            return 0;
        }
    }
}
