using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Interpreter.Structs;
using Interpreter.Vm;

namespace Interpreter.Libraries.Nori
{
    internal static class WindowUtil
    {
        private class NoriWindow : Form
        {
            public int BackgroundExecutionContextId { get; set; }
            public bool IsBlocking { get; set; }
            public Value OnLoadFunctionPointer { get; set; }
            public Value RenderFunctionPointer { get; set; }
            public Value InvokeMenuHandlerFunctionPointer { get; set; }

            public bool MenuShown { get; set; }
            public Panel MenuHost { get; set; }
            public Panel ContentHost { get; set; }

            private List<Value> sizeRelay;

            public NoriWindow(List<Value> sizeRelay) : base()
            {
                this.sizeRelay = sizeRelay;

                this.MenuShown = false;
                this.MenuHost = new Panel()
                {
                    Width = this.ClientSize.Width,
                    Height = 0,
                };
                this.ContentHost = new Panel()
                {
                    Width = this.ClientSize.Width,
                    Height = this.ClientSize.Height,
                };

                this.MainMenuStrip = new MenuStrip();
                this.MenuHost.Controls.Add(this.MainMenuStrip);

                this.Controls.Add(this.MenuHost);
                this.Controls.Add(this.ContentHost);

                this.Load += (sender, e) => { this.LoadHandler(); };
                this.FormClosed += (sender, e) => { this.PostCloseHandler(); };
                this.ClientSizeChanged += (sender, e) => { this.SizeChangedHandler(this.ClientSize.Width, this.ClientSize.Height); };

                this.Icon = IconHelper.GetDefaultIcon();
            }

            public void InvokeHandler(string token)
            {
                if (token != null && token.Length != 0)
                {
                    TranslationHelper.RunInterpreter(
                        this.InvokeMenuHandlerFunctionPointer,
                        new Value[] { CrayonWrapper.v_buildString(token) });
                }
            }

            private void LoadHandler()
            {
                TranslationHelper.RunInterpreter(this.OnLoadFunctionPointer);

                if (!this.IsBlocking)
                {
                    // Continue running the same VM context.
                    TranslationHelper.RunInterpreter(this.BackgroundExecutionContextId);
                }
            }

            private void PostCloseHandler()
            {
                if (this.IsBlocking)
                {
                    // Continue on with the VM context that launched this window.
                    TranslationHelper.RunInterpreter(this.BackgroundExecutionContextId);
                }
            }

            private void SizeChangedHandler(int newWidth, int newHeight)
            {
                int menuHeight = this.MenuShown ? this.MainMenuStrip.Height : 0;
                this.sizeRelay[0] = CrayonWrapper.v_buildInteger(newWidth);
                this.sizeRelay[1] = CrayonWrapper.v_buildInteger(newHeight - menuHeight);

                this.RunRenderer();
            }

            public void RunRenderer()
            {
                bool mainMenuPresent = this.MainMenuStrip != null;
                int width = this.ClientSize.Width;
                int height = this.ClientSize.Height;
                if (this.MenuShown)
                {
                    int menuHeight = this.MainMenuStrip.Height;
                    this.MenuHost.Width = width;
                    this.MenuHost.Height = menuHeight;
                    this.ContentHost.Width = width;
                    this.ContentHost.Height = height - menuHeight;
                    this.ContentHost.Location = new Point(0, menuHeight);
                }
                else
                {
                    this.MenuHost.Width = 0;
                    this.MenuHost.Height = 0;
                    this.ContentHost.Width = width;
                    this.ContentHost.Height = height;
                    this.ContentHost.Location = new Point(0, 0);
                }

                if (this.RenderFunctionPointer != null)
                {
                    TranslationHelper.RunInterpreter(this.RenderFunctionPointer);
                }
            }
        }

        public static void InstaniateWindow(object[] windowNativeData, object[] uiBoxNativeData, List<Value> sizeRelay)
        {
            NoriWindow window = new NoriWindow(sizeRelay);
            Panel uiBoxPanel = new Panel();
            window.ContentHost.Controls.Add(uiBoxPanel);
            windowNativeData[0] = window;
            uiBoxNativeData[0] = uiBoxPanel;
        }

        public static void ShowWindow(
            object nativeWindowInstance,
            string title,
            bool isBlocking,
            int currentExecutionContextId,
            Value renderFunctionPointer,
            Value onLoadFunctionPointer,
            Value invokeMenuHandlerFunctionPointer,
            int width,
            int height)
        {
            NoriWindow window = (NoriWindow)nativeWindowInstance;
            window.IsBlocking = isBlocking;
            window.RenderFunctionPointer = renderFunctionPointer;
            window.OnLoadFunctionPointer = onLoadFunctionPointer;
            window.InvokeMenuHandlerFunctionPointer = invokeMenuHandlerFunctionPointer;
            window.ClientSize = new Size(width, height);
            window.Text = title;

            TranslationHelper.RunInterpreter(renderFunctionPointer);
            if (isBlocking)
            {
                window.ShowDialog();
            }
            else
            {
                window.Show();
            }
        }

        public static void UpdateMenu(object windowObj, object[] menuData)
        {
            NoriWindow window = (NoriWindow)windowObj;
            MenuStrip strip = window.MainMenuStrip;
            window.MainMenuStrip.Items.Clear();
            // TODO: suspend layout.

            window.MenuShown = menuData != null && menuData.Length > 0;
            if (window.MenuShown)
            {
                for (int i = 0; i < menuData.Length; ++i)
                {
                    object[] menuItem = (object[])menuData[i];
                    if ((bool)menuItem[0])
                    {
                        strip.Items.Add(GenerateMenuItem(window, menuItem));
                    }
                }
            }

            window.RunRenderer();
        }

        private static ToolStripItem GenerateMenuItem(NoriWindow window, object[] menuItem)
        {
            if (!(bool)menuItem[0])
            {
                return new ToolStripSeparator();
            }

            string name = (string)menuItem[1];
            string token = (string)menuItem[5];
            ToolStripMenuItem item = new ToolStripMenuItem(name);
            item.Click += (sender, e) => { window.InvokeHandler(token); };
            if (menuItem[6] != null)
            {
                object[] children = (object[])menuItem[6];
                for (int i = 0; i < children.Length; ++i)
                {
                    object[] child = (object[])children[i];
                    item.DropDownItems.Add(GenerateMenuItem(window, child));
                }
            }
            return item;
        }
    }
}
