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
                TranslationHelper.RunInterpreter(this.RenderFunctionPointer);
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
        }

        public static void InstaniateWindow(object[] windowNativeData, object[] uiBoxNativeData)
        {
            NoriWindow window = new NoriWindow();
            System.Windows.Forms.Panel uiBoxPanel = new System.Windows.Forms.Panel();
            window.Controls.Add(uiBoxPanel);
            windowNativeData[0] = window;
            uiBoxNativeData[0] = uiBoxPanel;
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
    }
}
