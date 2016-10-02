using System.Collections.Generic;
using System.Windows.Forms;

namespace %%%PROJECT_ID%%%.Library.Nori
{
    internal static class LayoutRenderer
    {
        public static object RectangleLayout(
            object rectangleObj,
            object uiBoxObj,
            int x,
            int y,
            int width,
            int height)
        {
            Panel rectangle = rectangleObj as Panel;
            if (rectangleObj == null)
            {
                rectangle = new Panel();
                ((Panel)uiBoxObj).Controls.Add(rectangle);
            }

            rectangle.Location = new System.Drawing.Point(x, y);
            rectangle.Size = new System.Drawing.Size(width, height);

            return rectangle;
        }

        public static object TextAreaLayout(
            object textAreaObj,
            object uiBoxObj,
            int x,
            int y,
            int width,
            int height)
        {
            TextBox textArea = textAreaObj as TextBox;
            if (textArea == null)
            {
                textArea = new TextBox()
                {
                    Multiline = true,
                };
                ((Panel)uiBoxObj).Controls.Add(textArea);
            }

            textArea.Location = new System.Drawing.Point(x, y);
            textArea.Size = new System.Drawing.Size(width, height);

            return textArea;
        }

        public static void UiBoxLayout(
            object uiBoxObj,
            int width,
            int height,
            bool clipping)
        {
            Panel uiBox = (Panel)uiBoxObj;
            uiBox.Size = new System.Drawing.Size(width, height);
        }

        public static void UiBoxRemoveElements(object panelObj, object[] elementsToRemove)
        {
            Panel panel = (Panel)panelObj;
            HashSet<Control> removeThese = new HashSet<Control>();
            for (int i = 0; i < elementsToRemove.Length; ++i)
            {
                removeThese.Add((Control)elementsToRemove[i]);
            }

            for (int i = panel.Controls.Count - 1; i >= 0; --i)
            {
                if (removeThese.Contains(panel.Controls[i]))
                {
                    panel.Controls.RemoveAt(i);
                }
            }
        }
    }
}
