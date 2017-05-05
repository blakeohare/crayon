using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Interpreter.Vm;

namespace Interpreter.Libraries.Nori
{
    public static class IconHelper
    {
        private static System.Drawing.Icon defaultIconCache = null;
        internal static System.Drawing.Icon GetDefaultIcon()
        {
            if (defaultIconCache == null)
            {
                System.Drawing.Bitmap icon = ResourceReader.ReadIconResource("DefaultIcon.png");
                defaultIconCache = System.Drawing.Icon.FromHandle(icon.GetHicon());
            }
            return defaultIconCache;
        }
    }
}
