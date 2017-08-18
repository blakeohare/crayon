using System;

internal static class Graphics2DTextHelper
{
    public static object CreateNativeFont(int fontType, int fontClass, string fontId, int size, bool isBold, bool isItalic)
    {
        if (fontType == 1) // embedded font resource
        {
            return Interpreter.UniversalFont.FromResources(fontId, size, isBold, isItalic);
        }
        else if (fontType == 3) // system font
        {
            return Interpreter.UniversalFont.FromSystem(fontId, size, isBold, isItalic);
        }
        else
        {
            throw new NotImplementedException("Not implemented.");
        }
    }

    public static bool IsSystemFontAvailable(String name)
    {
        // TODO: call this function directly
        return Interpreter.UniversalFont.IsSystemFontAvailable(name);
    }

    public static Interpreter.UniversalBitmap RenderCharTile(object nativeFont, int charId, int[] sizeOut)
    {
        Interpreter.UniversalBitmap bmp = new Interpreter.UniversalBitmap((Interpreter.UniversalFont)nativeFont, (char)charId);
        sizeOut[0] = bmp.Width;
        sizeOut[1] = bmp.Height;

        double leftMarginRatio = bmp.IsCairo ? 0.0 : 0.125;
        double widthRatio = bmp.IsCairo ? 1.0 : 0.55;
        sizeOut[2] = (int)(bmp.Width * leftMarginRatio);
        sizeOut[3] = (int)(bmp.Width * widthRatio);

        return bmp;
    }

    public static Interpreter.UniversalBitmap GenerateTextureAndAllocateFloatInfo(object[][] nativeDatas, int[] coordinateInfo, int width, int height)
    {
        Interpreter.UniversalBitmap output = new Interpreter.UniversalBitmap(width, height);
        Interpreter.UniversalBitmap.DrawingSession g = output.GetActiveDrawingSession();
        int length = nativeDatas.Length;
        int tileX, tileY, tileWidth, tileHeight;
        object[] nativeData;
        for (int i = 0; i < length; ++i)
        {
            nativeData = nativeDatas[i];
            tileX = coordinateInfo[i * 4];
            tileY = coordinateInfo[i * 4 + 1];
            tileWidth = coordinateInfo[i * 4 + 2];
            tileHeight = coordinateInfo[i * 4 + 3];

            g.Draw((Interpreter.UniversalBitmap)nativeData[0], tileX, tileY, 0, 0, tileWidth, tileHeight);
			nativeData[5] = -1;
			nativeData[6] = null;
            nativeData[7] = tileX;
            nativeData[8] = tileY;
            nativeData[9] = tileX + tileWidth;
            nativeData[10] = tileY + tileHeight;

            nativeData[11] = output.Width;
            nativeData[12] = output.Height;
        }
        g.Flush();
        return output;
    }

    public static int LoadOpenGlTexture(object bitmapObj)
    {
        Interpreter.UniversalBitmap bitmap = (Interpreter.UniversalBitmap)bitmapObj;
        return Interpreter.Libraries.Game.GlUtil.ForceLoadTexture(bitmap);
    }
}
