using System;
using System.Collections.Generic;

internal static class Graphics2DTextHelper
{
    public static object CreateNativeFont(int fontType, int fontClass, string fontId, int size, bool isBold, bool isItalic)
    {
        if (fontType == 3) // system font
        {
            System.Drawing.FontStyle style = isBold && isItalic
                ? (System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic)
                : isBold
                    ? System.Drawing.FontStyle.Bold
                    : isItalic
                        ? System.Drawing.FontStyle.Italic
                        : System.Drawing.FontStyle.Regular;
            System.Drawing.Font font = new System.Drawing.Font(fontId, size, style, System.Drawing.GraphicsUnit.Pixel);
            return font;
        }
        else
        {
            throw new NotImplementedException("Not implemented.");
        }
    }

    public static bool IsFontResourceAvailable(string path)
    {
        throw new NotImplementedException();
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
}
