using System.Windows.Forms;

namespace %%%PROJECT_ID%%%.Library.Nori
{
    internal static class VisualsRenderer
    {
        public static void RectangleVisuals(
            object rectangleObj,
            int red,
            int green,
            int blue,
            int alpha)
        {
            Panel panel = (Panel)rectangleObj;
            panel.BackColor = System.Drawing.Color.FromArgb(alpha, red, green, blue);
        }

        public static void TextAreaVisuals(
            object textAreaObj)
        {

        }
    }
}
