using Interpreter.Structs;
using System;
using System.Collections.Generic;

namespace Interpreter.Libraries.Nori2
{
    public static class NoriHelper
    {
        public static void AddChildToParent(object parent, object child)
        {
            ((System.Windows.Forms.Panel)parent).Controls.Add((System.Windows.Forms.Control)child);
        }

        public static void CloseWindow(object window)
        {
            throw new NotImplementedException();
        }

        public static void EnsureParentLinkOrder(object window, object[] children)
        {
            throw new NotImplementedException();
        }

        private static readonly System.Drawing.Point DEFAULT_LOCATION = new System.Drawing.Point();
        private static readonly System.Drawing.Size DEFAULT_SIZE = new System.Drawing.Size();
        private static readonly System.Drawing.Color DEFAULT_COLOR = System.Drawing.Color.Transparent;

        public static object InstantiateElement(int type, object[] properties)
        {
            System.Drawing.Point location = DEFAULT_LOCATION;
            System.Drawing.Size size = DEFAULT_SIZE;
            System.Drawing.Color color = DEFAULT_COLOR;
            if (properties[0] != null)
            {
                location = new System.Drawing.Point((int)properties[0], (int)properties[1]);
                size = new System.Drawing.Size((int)properties[4], (int)properties[5]);
            }
            if (properties[16] != null)
            {
                List<Value> colors = (List<Value>)properties[16];
                int red = (int)colors[0].internalValue;
                int green = (int)colors[1].internalValue;
                int blue = (int)colors[2].internalValue;
                int alpha = (int)colors[3].internalValue;
                color = System.Drawing.Color.FromArgb(alpha, red, green, blue);
            }

            switch (type)
            {
                case 1: // Rectangle
                    System.Windows.Forms.Panel rect = new System.Windows.Forms.Panel();
                    rect.Location = location;
                    rect.Size = size;
                    rect.BackColor = color;
                    return rect;

                case 2: // Canvas
                    System.Windows.Forms.Panel canvas = new System.Windows.Forms.Panel();
                    canvas.Location = location;
                    canvas.Size = size;
                    return canvas;

                case 3: // ScrollPanel
                    System.Windows.Forms.Panel scrollPanel = new System.Windows.Forms.Panel();
                    scrollPanel.AutoScroll = true;
                    scrollPanel.Location = location;
                    scrollPanel.Size = size;
                    return scrollPanel;

                default:
                    throw new NotImplementedException();
            }
        }

        public static object InstantiateWindow(object[] properties)
        {
            System.Windows.Forms.Form form = new System.Windows.Forms.Form();
            string title = properties[0].ToString();
            List<Value> size = (List<Value>)properties[1];
            int width = (int)size[0].internalValue;
            int height = (int)size[1].internalValue;
            form.Text = title;
            form.Size = new System.Drawing.Size(width, height);
            return form;
        }

        public static void InvalidateElementProperty(int type, object element, int key, object value)
        {
            throw new NotImplementedException();
        }

        public static void InvalidateWindowProperty(object window, int key, object value)
        {
            throw new NotImplementedException();
        }

        public static void ShowWindow(object window, object[] ignored)
        {
            System.Windows.Forms.Form form = (System.Windows.Forms.Form)window;
            form.ShowDialog();
        }
    }
}
