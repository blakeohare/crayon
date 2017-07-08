using System;
using System.Collections.Generic;

internal static class Graphics2DTextHelper
{
    public static object CreateNativeFont(int fontType, int fontClass, string fontId, int size, bool isBold, bool isItalic)
    {
        System.Drawing.FontStyle style = System.Drawing.FontStyle.Regular;
        if (isBold && isItalic) style = System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic;
        else if (isBold) style = System.Drawing.FontStyle.Bold;
        else if (isItalic) style = System.Drawing.FontStyle.Italic;
        System.Drawing.Font font;
        if (fontType == 1) // embedded font resource
        {
            // font ID is already converted into a canonical resource path
            font = Interpreter.ResourceReader.ReadFontResource(fontId, size, style);
        }
        else if (fontType == 3) // system font
        {
            font = new System.Drawing.Font(fontId, size, style, System.Drawing.GraphicsUnit.Pixel);
        }
        else
        {
            throw new NotImplementedException("Not implemented.");
        }

        return font;
    }

    private static readonly Dictionary<string, bool> systemFontCache = new Dictionary<string, bool>();
    public static bool IsSystemFontAvailable(String name)
    {
        if (systemFontCache.ContainsKey(name)) return systemFontCache[name];

        // .NET uses automatic font fallback. If the name of the font is different, it's not available.
        System.Drawing.Font dummyFont = new System.Drawing.Font(name, 12, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
        bool isAvailable = dummyFont.Name == name;
        systemFontCache[name] = isAvailable;
        return isAvailable;
    }

    public static object RenderTextToSurface(
        int[] sizeOut,
        object fontObj,
        int red,
        int green,
        int blue,
        string text)
    {

        throw new NotImplementedException();
    }

    public static Interpreter.UniversalBitmap RenderCharTile(object nativeFont, int charId, int[] sizeOut)
    {
        Interpreter.UniversalBitmap bmp = new Interpreter.UniversalBitmap((System.Drawing.Font)nativeFont, (char)charId);
        sizeOut[0] = bmp.Width;
        sizeOut[1] = bmp.Height;
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
            nativeData[4] = tileX;
            nativeData[5] = tileY;
            nativeData[6] = tileX + tileWidth;
            nativeData[7] = tileY + tileHeight;

            nativeData[10] = output.Width;
            nativeData[11] = output.Height;
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
