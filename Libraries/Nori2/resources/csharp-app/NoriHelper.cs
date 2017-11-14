using Interpreter.Structs;
using System;
using System.Collections.Generic;

namespace Interpreter.Libraries.Nori2
{
    public static class NoriHelper
    {
        public static void AddChildToParent(object child, object parent)
        {
            ((System.Windows.Forms.Panel)parent).Controls.Add((System.Windows.Forms.Control)child);
        }

        public static void CloseWindow(object window)
        {
            throw new NotImplementedException();
        }

        public static void EnsureParentLinkOrder(object parent, object[] children)
        {
            System.Windows.Forms.Control.ControlCollection actualCollection = ((System.Windows.Forms.Control)parent).Controls;
            System.Windows.Forms.Control[] actualOrder = GetControls(actualCollection);

            int perfectMatchUntil = 0;
            if (actualOrder.Length == children.Length)
            {
                for (int i = 0; i < children.Length; ++i)
                {
                    if (children[i] == actualOrder[i])
                    {
                        perfectMatchUntil = i + 1;
                    }
                }
                if (perfectMatchUntil == children.Length)
                {
                    return;
                }
            }
            throw new NotImplementedException();
        }

        public static object InstantiateElement(int type, object[] properties)
        {
            int left = 0;
            int top = 0;
            int width = 0;
            int height = 0;
            int red = 0;
            int green = 0;
            int blue = 0;
            int alpha = 0;

            if (properties[0] != null)
            {
                left = (int)properties[0];
                top = (int)properties[1];
                width = (int)properties[4];
                height = (int)properties[5];
            }
            if (properties[16] != null)
            {
                List<Value> colors = (List<Value>)properties[16];
                red = (int)colors[0].internalValue;
                green = (int)colors[1].internalValue;
                blue = (int)colors[2].internalValue;
                alpha = (int)colors[3].internalValue;
            }

            switch (type)
            {
                case 1: // Rectangle
                    Rectangle rect = new Rectangle();
                    rect.SetPosition(left, top, width, height);
                    rect.SetColor(red, green, blue, alpha);
                    return rect;

                case 2: // Canvas
                    Canvas canvas = new Canvas();
                    canvas.SetPosition(left, top, width, height);
                    return canvas;

                case 3: // ScrollPanel
                    ScrollPanel scrollPanel = new ScrollPanel();
                    scrollPanel.SetPosition(left, top, width, height);
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

        public static void ShowWindow(object window, object[] ignored, object rootElement)
        {
            System.Windows.Forms.Form form = (System.Windows.Forms.Form)window;
            form.Controls.Add((System.Windows.Forms.Panel)rootElement);
            form.ShowDialog();
        }

        public static System.Windows.Forms.Control[] GetControls(System.Windows.Forms.Control.ControlCollection cc)
        {
            List<System.Windows.Forms.Control> output = new List<System.Windows.Forms.Control>();
            foreach (System.Windows.Forms.Control c in cc)
            {
                output.Add(c);
            }
            return output.ToArray();
        }

        public static void UpdateLayout(object element, int typeId, int x, int y, int width, int height)
        {
            switch (typeId)
            {
                case 1:
                    Rectangle r = (Rectangle)element;
                    r.Location = new System.Drawing.Point(x, y);
                    r.Size = new System.Drawing.Size(width, height);
                    break;

                case 3:
                    ScrollPanel sp = (ScrollPanel)element;
                    sp.Location = new System.Drawing.Point(x, y);
                    sp.Size = new System.Drawing.Size(width, height);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
