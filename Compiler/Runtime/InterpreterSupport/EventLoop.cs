using Interpreter.Structs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Vm
{
    public class EventLoop
    {
        private VmContext vm;
        private Func<bool> completionCallback;

        public EventLoop(VmContext vm, Func<bool> completionCallback)
        {
            this.vm = vm;
            this.completionCallback = completionCallback;
            CrayonWrapper.vmSetEventLoopObj(vm, this);
        }

        private class EventLoopInvocation
        {
            public bool StartFromBeginning { get; set; }
            public double Timestamp { get; set; }
            public Value FunctionPointer { get; set; }
            public Value[] FunctionPointerArgs { get; set; }
            public object[] FunctionPointerNativeArgs { get; set; }
            public int ExecutionContextId { get; set; }
        }

        private List<EventLoopInvocation> queue = new List<EventLoopInvocation>();

        public void ResumeExecution(int executionContextId)
        {
            AddItemToQueue(new EventLoopInvocation()
            {
                ExecutionContextId = executionContextId,
            });
        }

        public void ExecuteFunctionPointer(Value fp, IList<Value> args)
        {
            AddItemToQueue(new EventLoopInvocation()
            {
                FunctionPointer = fp,
                FunctionPointerArgs = args.ToArray(),
            });
        }

        public void ExecuteFunctionPointerNativeArgs(Value fp, IList<object> args)
        {
            AddItemToQueue(new EventLoopInvocation()
            {
                FunctionPointer = fp,
                FunctionPointerNativeArgs = args.ToArray(),
            });
        }

        public void ExecuteFunctionPointerWithDelay(Value fp, double delay)
        {
            AddItemToQueue(new EventLoopInvocation()
            {
                FunctionPointer = fp,
                FunctionPointerArgs = new Value[0],
                Timestamp = TranslationHelper.GetCurrentTime() + delay,
            });
        }

        public void ResumeExecutionAfterDelay(int executionContextId, double delay)
        {
            if (delay <= 0) ResumeExecution(executionContextId);
            else
            {
                AddItemToQueue(new EventLoopInvocation()
                {
                    ExecutionContextId = executionContextId,
                    Timestamp = TranslationHelper.GetCurrentTime() + delay,
                });
            }
        }

        private void AddItemToQueue(EventLoopInvocation invocation)
        {
            if (invocation.Timestamp == 0)
            {
                invocation.Timestamp = TranslationHelper.GetCurrentTime() - 0.0000001;
            }
            lock (queue)
            {
                queue.Add(invocation);
            }
        }

        private EventLoopInvocation PopItemFromQueue()
        {
            double currentTime = TranslationHelper.GetCurrentTime();
            EventLoopInvocation lowest = null;
            int lowestIndex = -1;
            lock (queue)
            {
                for (int i = 0; i < queue.Count; ++i)
                {
                    EventLoopInvocation item = queue[i];
                    if (item.Timestamp < currentTime && (lowest == null || item.Timestamp < lowest.Timestamp))
                    {
                        lowestIndex = i;
                        lowest = item;
                    }
                }

                if (lowest != null)
                {
                    queue.RemoveAt(lowestIndex);
                }
            }

            return lowest;
        }

        private int startingThreadId = 0;
        private int GetThreadId() { return System.Threading.Thread.CurrentThread.ManagedThreadId; }

        public void EnsureRunningOnStartingThread()
        {
            if (GetThreadId() != startingThreadId)
            {
                throw new Exception("The VM cannot be invoked on a separate thread.");
            }
        }

        public void StartInterpreter()
        {
            if (startingThreadId != 0) throw new Exception();
            startingThreadId = GetThreadId();
            AddItemToQueue(new EventLoopInvocation()
            {
                StartFromBeginning = true,
            });
            RunEventLoop();
        }

        private Value ConvertNativeArg(object na)
        {
            if (na == null) return vm.globalNull;
            if (na is Value) return (Value)na; // allow mixed native/Value
            if (na is bool) return (bool)na ? vm.globalTrue : vm.globalFalse;
            if (na is string) return CrayonWrapper.buildString(vm.globals, (string)na);
            if (na is int) return CrayonWrapper.buildInteger(vm.globals, (int)na);
            if (na is double) return CrayonWrapper.buildFloat(vm.globals, (double)na);
            if (na is float) return CrayonWrapper.buildFloat(vm.globals, (float)na);
            if (na is object[])
            {
                List<Value> list = new List<Value>(((object[])na).Select(a => ConvertNativeArg(a)));
                return CrayonWrapper.buildList(list);
            }

            throw new NotImplementedException("Unsupported type for native arg: " + na.GetType());
        }

        private void RunEventLoopIteration(EventLoopInvocation invocation)
        {
            EnsureRunningOnStartingThread();

            // Debugger.INSTANCE.FlushMessageQueue();

            if (invocation == null) return;

            InterpreterResult result;
            if (invocation.StartFromBeginning)
            {
                result = CrayonWrapper.startVm(vm);
            }
            else if (invocation.FunctionPointer != null)
            {
                Value[] args = invocation.FunctionPointerArgs;
                if (args == null)
                {
                    object[] nativeArgs = invocation.FunctionPointerNativeArgs;
                    args = new Value[nativeArgs.Length];
                    for (int i = 0; i < args.Length; ++i)
                    {
                        args[i] = ConvertNativeArg(nativeArgs[i]);
                    }
                }
                result = CrayonWrapper.runInterpreterWithFunctionPointer(vm, invocation.FunctionPointer, args);
            }
            else
            {
                result = CrayonWrapper.runInterpreter(vm, invocation.ExecutionContextId);
            }

            switch (result.status)
            {
                case 1: // execution context is FINISHED
                    if (result.isRootContext) this.eventLoopAlive = false;
                    break;

                case 2: // SUSPEND
                    // do nothing.
                    break;

                case 3: // FATAL ERROR
                    if (result.isRootContext) this.eventLoopAlive = false;
                    break;

                case 5: // RE-INVOKE, possibly with a delay
                    ResumeExecutionAfterDelay(result.executionContextId, result.reinvokeDelay);
                    break;

                case 7: // BREAKPOINT
                    // do nothing
                    break;
            }

            if (!this.eventLoopAlive && this.completionCallback != null)
            {
                this.completionCallback();
                this.completionCallback = null;
            }
        }

        private bool eventLoopAlive = true;

        public void RunEventLoop()
        {
            while (this.eventLoopAlive)
            {
                EventLoopInvocation invocation = PopItemFromQueue();
                if (invocation != null)
                {
                    RunEventLoopIteration(invocation);
                }
                else
                {
                    // TODO: Check if root execution context has ended.
                }

                if (invocation == null)
                {
                    // This is about half a millisecond I have determined on this particular computer I'm
                    // sitting at right now. I may want to derive this at runtime, though.
                    // Thread.Sleep() is inaccurate because the OS thread scheduler takes a non-trivial
                    // amount of time.
                    int aboutHalfAMillisecond = 100000;
                    System.Threading.Thread.SpinWait(aboutHalfAMillisecond);
                }
            }
        }
    }
}
