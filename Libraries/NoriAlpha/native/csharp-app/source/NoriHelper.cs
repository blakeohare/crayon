using System.Collections.Generic;

namespace Interpreter.Libraries.NoriAlpha
{
    public class NoriHelper
    {
        public static object ShowFrame(Interpreter.Structs.Value crayonObj, string title, int width, int height, string uiData)
        {
            NoriFrame frame = NoriFrame.CreateAndShow(crayonObj, title, width, height, uiData);
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

        public static void QueueEventMessage(NoriFrame sender, int elementId, string value)
        {
            lock (eventQueue)
            {
                eventQueue.Add("EVENT");
                eventQueue.Add(sender);
                eventQueue.Add(elementId);
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
            Interpreter.Structs.Value noriHandlerFunctionPointer)
        {
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
                    for (int i = 0; i < eventQueue.Count; i += 4)
                    {
                        string type = eventQueue[i].ToString();
                        NoriFrame sender = (NoriFrame)eventQueue[i + 1];
                        if (type == "EVENT")
                        {
                            int elementId = (int)eventQueue[i + 2];
                            string args = (string)eventQueue[i + 3];
                            Interpreter.Vm.CrayonWrapper.runInterpreterWithFunctionPointer(
                               vmContext,
                               noriHandlerFunctionPointer,
                               new Interpreter.Structs.Value[] {
                               sender.CrayonObjectRef,
                               Interpreter.Vm.CrayonWrapper.buildInteger(vmContext.globals, elementId),
                               Interpreter.Vm.CrayonWrapper.buildString(vmContext.globals, args)
                               });
                        }
                        else if (type == "CLOSE")
                        {
                            if (activeFrames.Contains(sender))
                            {
                                activeFrames.Remove(sender);
                            }
                        }
                    }
                }
                else
                {
                    System.Threading.Thread.Sleep(System.TimeSpan.FromMilliseconds(0.1));
                }
            }
        }

        public static bool CloseFrame(object nativeFrameHandle)
        {
            NoriFrame frame = (NoriFrame) nativeFrameHandle;
            frame.Close();
            return true;
        }

        public static bool FlushUpdatesToFrame(object nativeFrameHandle, string uiData)
        {
            throw new System.NotImplementedException();
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
