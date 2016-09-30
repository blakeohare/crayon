using System;
using System.Collections.Generic;

namespace %%%PROJECT_ID%%%.Library.Nori
{
    internal static class WindowUtil
    {
        private class NoriWindow : System.Windows.Forms.Form
        {
            public int BackgroundExecutionContextId { get; set; }
            public bool IsBlocking { get; set; }
            public Value OnLoadFunctionPointer { get; set; }
            public Value RenderFunctionPointer { get; set; }

            public NoriWindow() : base()
            {
                this.Load += (sender, e) => { this.LoadHandler(); };
                this.FormClosed += (sender, e) => { this.PostCloseHandler(); };
            }

            private void LoadHandler()
            {
                CrayonWrapper.v_runInterpreterWithFunctionPointer(this.OnLoadFunctionPointer, new Value[0]);
                if (!this.IsBlocking)
                {
                    // Continue running the same VM context.
                    CrayonWrapper.v_runInterpreter(this.BackgroundExecutionContextId);
                }
            }

            private void PostCloseHandler()
            {
                if (this.IsBlocking)
                {
                    // Continue on with the VM context that launched this window.
                    CrayonWrapper.v_runInterpreter(this.BackgroundExecutionContextId);
                }
            }
        }

        public static object InstaniateWindow()
        {
            return new NoriWindow();
        }

        public static void ShowWindow(
            object nativeWindowInstance,
            string title,
            bool isBlocking,
            int currentExecutionContextId,
            Value renderFunctionPointer,
            Value onLoadFunctionPointer)
        {
            NoriWindow window = (NoriWindow)nativeWindowInstance;
            window.IsBlocking = isBlocking;
            window.RenderFunctionPointer = renderFunctionPointer;
            window.OnLoadFunctionPointer = onLoadFunctionPointer;

            CrayonWrapper.v_runInterpreterWithFunctionPointer(renderFunctionPointer, new Value[0]);
            if (isBlocking)
            {
                window.ShowDialog();
            }
            else
            {
                window.Show();
            }
        }
    }
}
