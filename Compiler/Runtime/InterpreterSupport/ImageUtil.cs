﻿using System;
using System.Collections.Generic;
using System.Linq;
using Interpreter.Structs;

namespace Interpreter.Vm
{
    public static class ImageUtil
    {
        public static object NewBitmap(int width, int height)
        {
            return new Wax.Util.Images.UniversalBitmap(width, height);
        }

        public static void ChunkLoadAsync(VmContext vm, Dictionary<int, object> loadedChunks, int chunkId, int[] chunkIds, Value loadedCallback)
        {
            string path = "ch_" + chunkId + ".png";
            loadedChunks[chunkId] = ((ResourceReader)vm.environment.resourceReader).ReadImageResource(path);
            if (loadedChunks[chunkId] == null)
            {
                throw new Exception("Image resource not available: " + path);
            }
            int total = chunkIds.Length;
            int loaded = 0;
            foreach (int otherChunk in chunkIds)
            {
                if (loadedChunks.ContainsKey(otherChunk)) loaded++;
            }
            TranslationHelper.GetEventLoop(vm).ExecuteFunctionPointerNativeArgs(loadedCallback, new object[] { chunkId, loaded, total });
        }

        public static object Scale(object rawBmp, int newWidth, int newHeight, int algo)
        {
            if (algo != 1) throw new NotImplementedException();
            Wax.Util.Images.UniversalBitmap original = (Wax.Util.Images.UniversalBitmap)rawBmp;
            Wax.Util.Images.UniversalBitmap target = new Wax.Util.Images.UniversalBitmap(newWidth, newHeight);
            target.CreateNewDrawingSession().Draw(original, 0, 0, 0, 0, newWidth, newHeight, original.Width, original.Height).Flush();
            target.ClearBuffer();
            return target;
        }

        public static object StartEditSession(object bmpObj)
        {
            Wax.Util.Images.UniversalBitmap bmp = (Wax.Util.Images.UniversalBitmap)bmpObj;
            Wax.Util.Images.UniversalBitmap.DrawingSession session = bmp.CreateNewDrawingSession();
            return session;
        }

        public static void EndEditSession(object sessionObj, object bmpObj)
        {
            Wax.Util.Images.UniversalBitmap.DrawingSession session = (Wax.Util.Images.UniversalBitmap.DrawingSession)sessionObj;
            session.Flush();
        }

        public static void Blit(object targetObj, object srcObj, int sx, int sy, int sw, int sh, int tx, int ty, int tw, int th)
        {
            Wax.Util.Images.UniversalBitmap.DrawingSession target = (Wax.Util.Images.UniversalBitmap.DrawingSession)targetObj;
            Wax.Util.Images.UniversalBitmap src = (Wax.Util.Images.UniversalBitmap)srcObj;
            target.Draw(src, tx, ty, sx, sy, tw, th, sw, sh);
        }

        public static void GetPixel(object bmpRaw, object nullableEditSession, int x, int y, int[] colorOut)
        {
            Wax.Util.Images.UniversalBitmap bmp = (Wax.Util.Images.UniversalBitmap)bmpRaw;
            if (x < 0 || y < 0 || x >= bmp.Width || y >= bmp.Height)
            {
                colorOut[4] = 0;
                return;
            }
            colorOut[4] = 1;
            bmp.GetPixel(x, y, colorOut);
            return;
        }

        // returns true for out of range
        public static bool SetPixel(object sessionObj, int x1, int y1, int x2, int y2, int r, int g, int b, int a)
        {
            Wax.Util.Images.UniversalBitmap.DrawingSession session = (Wax.Util.Images.UniversalBitmap.DrawingSession)sessionObj;
            if (x1 < 0 || y1 < 0 || x1 >= session.Parent.Width || y1 >= session.Parent.Height) return true;
            if (x2 < 0 || y2 < 0 || x2 >= session.Parent.Width || y2 >= session.Parent.Height) return true;

            if (x1 == x2 && y1 == y2)
            {
                session.SetPixel(x1, y1, r, g, b, a);
            }
            else
            {
                int xdiff = x2 - x1;
                int ydiff = y2 - y1;
                double progress;
                int step, xEnd, yEnd, x, y;
                if (Math.Abs(xdiff) > Math.Abs(ydiff))
                {
                    // draw horizontally
                    step = xdiff > 0 ? 1 : -1;
                    xEnd = x2 + step;
                    for (x = x1; x != xEnd; x += step)
                    {
                        progress = 1.0 * (x - x1) / (x2 - x1);
                        y = (int)(progress * ydiff + .5) + y1;
                        session.SetPixel(x, y, r, g, b, a);
                    }
                }
                else
                {
                    // draw vertically
                    step = ydiff > 0 ? 1 : -1;
                    yEnd = y2 + step;
                    for (y = y1; y != yEnd; y += step)
                    {
                        progress = 1.0 * (y - y1) / (y2 - y1);
                        x = (int)(progress * xdiff + .5) + x1;
                        session.SetPixel(x, y, r, g, b, a);
                    }
                }
            }
            return false;
        }

        public static bool FromBytes(int[] bytesAsInts, int[] sizeOut, object[] nativeDataOut)
        {
            Wax.Util.Images.UniversalBitmap bmp = new Wax.Util.Images.UniversalBitmap(bytesAsInts.Select(b => (byte)b).ToArray());
            if (!bmp.IsValid)
            {
                nativeDataOut[0] = null;
            }
            else
            {
                sizeOut[0] = bmp.Width;
                sizeOut[1] = bmp.Height;
                nativeDataOut[0] = bmp;
            }
            return true;
        }

        public static object Encode(object bmpObj, int format, bool[] formatOut)
        {
            Wax.Util.Images.UniversalBitmap bmp = (Wax.Util.Images.UniversalBitmap)bmpObj;
            byte[] bytes;
            switch (format)
            {
                case 1: bytes = bmp.GetBytesAsPng(); break;
                case 2: bytes = bmp.GetBytesAsJpeg(); break;
                default: throw new Exception();
            }
            formatOut[0] = false;
            return bytes.Select(b => (int)b).ToArray();
        }
    }
}
