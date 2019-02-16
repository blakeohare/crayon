using System.Collections.Generic;

namespace Interpreter.Libraries.Nori
{
    public class NoriHelper
    {
        public static object ShowFrame(Interpreter.Structs.Value crayonObj, string title, int width, int height, string uiData, int execId)
        {
            NoriFrame frame = NoriFrame.CreateAndShow(crayonObj, title, width, height, uiData, execId);
            activeFrames.Add(frame);
            return frame;
        }

        /* 
            The various JS Bridges and frames will push to this queue.
            Each will push 4 items: 
                [0] -> string -> event type { CLOSE | EVENT }
                [1] -> NoriFrame -> sender
                [2] -> int -> object ID of sender (or 0 in event of frame event)
                [3] -> string -> args for event or empty string

        */
        private static List<object> eventQueue = new List<object>();

        public static void QueueEventMessage(NoriFrame sender, int elementId, string eventName, string value)
        {
            lock (eventQueue)
            {
                eventQueue.Add("EVENT");
                eventQueue.Add(sender);
                eventQueue.Add(elementId);
                eventQueue.Add(eventName);
                eventQueue.Add(value);
            }
        }

        public static void QueueCloseWindowNotification(NoriFrame sender)
        {
            lock (eventQueue)
            {
                eventQueue.Add("CLOSE");
                eventQueue.Add(sender);
                eventQueue.Add(0);
                eventQueue.Add("");
                eventQueue.Add("");
            }
        }

        private static HashSet<NoriFrame> activeFrames = new HashSet<NoriFrame>();

        /*
            Called by a CNI function after the frame is shown and just blocks and waits for
            events from the queue.
            If a window close event is generated, then blocking ends.
         */
        public static void EventWatcher(
            Interpreter.Structs.VmContext vmContext,
            int resumingExecutionContextId,
            Interpreter.Structs.Value openedFrameAsValue,
            Interpreter.Structs.Value noriHandlerFunctionPointer,
            Interpreter.Structs.Value postShowFunctionPointer)
        {
            Interpreter.Vm.CrayonWrapper.runInterpreterWithFunctionPointer(
               vmContext,
               postShowFunctionPointer,
               new Interpreter.Structs.Value[] { openedFrameAsValue });

            // TODO: find a better way.
            while (activeFrames.Count > 0)
            {
                object[] events = null;
                lock (eventQueue)
                {
                    if (eventQueue.Count > 0)
                    {
                        events = eventQueue.ToArray();
                        eventQueue.Clear();
                    }
                }

                if (events != null)
                {
                    for (int i = 0; i < events.Length; i += 5)
                    {
                        string type = events[i].ToString();
                        NoriFrame sender = (NoriFrame)events[i + 1];
                        if (type == "EVENT")
                        {
                            int elementId = (int)events[i + 2];
                            string eventName = (string)events[i + 3];
                            string args = (string)events[i + 4];
                            Interpreter.Vm.CrayonWrapper.runInterpreterWithFunctionPointer(
                               vmContext,
                               noriHandlerFunctionPointer,
                               new Interpreter.Structs.Value[] {
                                   sender.CrayonObjectRef,
                                   Interpreter.Vm.CrayonWrapper.buildInteger(vmContext.globals, elementId),
                                   Interpreter.Vm.CrayonWrapper.buildString(vmContext.globals, eventName),
                                   Interpreter.Vm.CrayonWrapper.buildString(vmContext.globals, args)
                               });
                        }
                        else if (type == "CLOSE")
                        {
                            if (activeFrames.Contains(sender))
                            {
                                activeFrames.Remove(sender);
                            }

                            if (resumingExecutionContextId != sender.ResumeExecId)
                            {
                                Interpreter.Vm.CrayonWrapper.runInterpreter(vmContext, sender.ResumeExecId);
                            }
                        }
                    }
                }
                else
                {
                    System.Threading.Thread.Sleep(System.TimeSpan.FromMilliseconds(0.1));
                }
            }

            Structs.ExecutionContext ec = Interpreter.Vm.CrayonWrapper.getExecutionContext(vmContext, resumingExecutionContextId);
            ec.activeInterrupt = null;
            ec.executionStateChange = false;
        }

        public static bool CloseFrame(object nativeFrameHandle)
        {
            NoriFrame frame = (NoriFrame)nativeFrameHandle;
            frame.Close();
            return true;
        }

        public static bool FlushUpdatesToFrame(object nativeFrameHandle, string uiData)
        {
            NoriFrame frame = (NoriFrame)nativeFrameHandle;
            frame.SendUiData(uiData);
            return true;
        }

        public static void SendImageToRenderer(object frameObj, int id, object nativeImageData, int x, int y, int width, int height)
        {
            NoriFrame frame = (NoriFrame)frameObj;
            UniversalBitmap atlas = (UniversalBitmap)nativeImageData;
            UniversalBitmap cropped;
            if (atlas.Width == width && atlas.Height == height)
            {
                cropped = atlas;
            }
            else
            {
                cropped = new UniversalBitmap(width, height);
                UniversalBitmap.DrawingSession session = cropped.GetActiveDrawingSession();
                session.Draw(atlas, 0, 0, x, y, width, height);
                session.Flush();
            }
            byte[] pngBytes = cropped.GetBytesAsPng();
            string base64Image = UniversalBitmap.ToBase64("data:image/png;base64,", pngBytes);
            frame.SendImageToBrowser(id, width, height, base64Image);
        }

        private static char[] HEX = "0123456789ABCDEF".ToCharArray();
        public static string EscapeStringHex(string original)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            int len = original.Length;
            char c;
            int a, b;
            for (int i = 0; i < len; ++i)
            {
                c = original[i];
                a = ((int)c >> 4) & 255;
                b = ((int)c & 15);
                sb.Append(HEX[a]);
                sb.Append(HEX[b]);
            }
            return sb.ToString();
        }
    }
}
