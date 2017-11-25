using Interpreter.Structs;
using System;
using System.Collections.Generic;

namespace Interpreter.Libraries.Nori
{
    public static class NoriHelper
    {
        public static void RegisterHandler(object element, int typeId, string type, int handlerId)
        {
            switch (typeId + ":" + type)
            {
                case "4:click": // button click
                    Element.Button btn = (Element.Button)element;
                    btn.Click += (obj, args) => { EventHandlerCallback(handlerId); };
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private static void EventHandlerCallback(int id)
        {
            ARGS_WRAPPER[0] = Interpreter.Vm.CrayonWrapper.v_buildInteger(id);
            Interpreter.Vm.CrayonWrapper.v_runInterpreterWithFunctionPointer(EVENT_HANDLER_CALLBACK, ARGS_WRAPPER);
        }

        private static Value[] ARGS_WRAPPER = new Value[1];
        private static Value EVENT_HANDLER_CALLBACK = null;
        private static List<Value> EVENT_HANDLER_ARGS_OUT = null;
        public static void RegisterHandlerCallback(Value callback, List<Value> args)
        {
            EVENT_HANDLER_CALLBACK = callback;
            EVENT_HANDLER_ARGS_OUT = args;
        }

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

        public static object InstantiateElement(int type, ElementProperties properties)
        {
            int left = properties.render_left;
            int top = properties.render_top;
            int width = properties.render_width;
            int height = properties.render_height;

            switch (type)
            {
                case 1: // Rectangle
                    Element.Rectangle rect = new Element.Rectangle();
                    rect.SetColor(
                        properties.bg_red,
                        properties.bg_green,
                        properties.bg_blue,
                        properties.bg_alpha);
                    return rect;

                case 2: // Canvas
                    Element.Canvas canvas = new Element.Canvas();
                    canvas.SetPosition(left, top, width, height);
                    return canvas;

                case 3: // ScrollPanel
                    Element.ScrollPanel scrollPanel = new Element.ScrollPanel();
                    scrollPanel.SetPosition(left, top, width, height);
                    return scrollPanel;

                case 4: // Button
                    Element.Button button = new Element.Button();
                    button.SetPosition(left, top, width, height);
                    button.Text = properties.misc_string_0;
                    return button;

                case 5: // Label
                    Element.Label label = new Element.Label();
                    label.Text = properties.misc_string_0;
                    return label;

                default:
                    throw new NotImplementedException();
            }
        }

        public static object InstantiateWindow(WindowProperties properties)
        {
            System.Windows.Forms.Form form = new System.Windows.Forms.Form();
            form.Text = properties.title;
            form.Size = new System.Drawing.Size(properties.width, properties.height);
            return form;
        }

        public static void InvalidateElementProperty(int type, object element, int key, object value)
        {
            switch (type)
            {
                case 4:
                    switch (key)
                    {
                        case 21: ((Element.Button)element).Text = value.ToString(); return;
                    }
                    break;
                case 5:
                    switch (key)
                    {
                        case 21: ((Element.Label)element).Text = value.ToString(); return;
                    }
                    break;
                default:
                    break;
            }
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
                    Element.Rectangle r = (Element.Rectangle)element;
                    r.SetPosition(x, y, width, height);
                    break;

                case 3:
                    Element.ScrollPanel sp = (Element.ScrollPanel)element;
                    sp.SetPosition(x, y, width, height);
                    break;

                case 4:
                    Element.Button btn = (Element.Button)element;
                    btn.SetPosition(x, y, width, height);
                    break;

                case 5:
                    Element.Label lbl = (Element.Label)element;
                    lbl.SetPosition(x, y, width, height);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
