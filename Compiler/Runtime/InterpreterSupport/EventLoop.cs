using Interpreter.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Interpreter.Vm
{
    public class EventLoop
    {
        private class EventLoopConcurrentState
        {
            public Queue<EventLoopInvocation> Queue { get; set; } = new Queue<EventLoopInvocation>();
            public bool IsStillRunning { get; set; } = true;
        }

        private VmContext vm;
        private Func<bool> completionCallback;

        public EventLoop(VmContext vm)
        {
            this.vm = vm;
            CrayonWrapper.vmSetEventLoopObj(vm, this);
        }

        private class EventLoopInvocation
        {
            public bool StartFromBeginning { get; set; }
            public Value FunctionPointer { get; set; }
            public Value[] FunctionPointerArgs { get; set; }
            public object[] FunctionPointerNativeArgs { get; set; }
            public int ExecutionContextId { get; set; }
        }

        private EventLoopConcurrentState state = new EventLoopConcurrentState();

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
            Task.Delay((int)(delay * 1000)).ContinueWith(_ =>
            {
                AddItemToQueue(new EventLoopInvocation()
                {
                    FunctionPointer = fp,
                    FunctionPointerArgs = new Value[0],
                });
            });
        }

        public void ResumeExecutionAfterDelay(int executionContextId, double delay)
        {
            if (delay <= 0) ResumeExecution(executionContextId);
            else
            {
                Task.Delay((int)(delay * 1000)).ContinueWith(_ =>
                {
                    AddItemToQueue(new EventLoopInvocation()
                    {
                        ExecutionContextId = executionContextId,
                    });
                });
            }
        }

        private void AddItemToQueue(EventLoopInvocation invocation)
        {
            lock (this.state)
            {
                if (!this.state.IsStillRunning) return;
                this.state.Queue.Enqueue(invocation);
            }
            lock (this.wakeupTaskMutex)
            {
                if (this.wakeupTask != null) // Root execution context starting will not have a wakup task to trigger
                {
                    this.wakeupTask.TrySetResult(true);
                }
            }
        }

        private EventLoopInvocation PopItemFromQueue()
        {
            lock (this.state)
            {
                if (this.state.Queue.Count > 0)
                {
                    return this.state.Queue.Dequeue();
                }
            }

            return null;
        }

        public Task StartInterpreter()
        {
            AddItemToQueue(new EventLoopInvocation()
            {
                StartFromBeginning = true,
            });
            return RunEventLoop();
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
                    if (result.isRootContext) this.KillEventLoop();
                    break;

                case 2: // SUSPEND
                    // do nothing.
                    break;

                case 3: // FATAL ERROR
                    if (result.isRootContext) this.KillEventLoop();
                    break;

                case 5: // RE-INVOKE, possibly with a delay
                    ResumeExecutionAfterDelay(result.executionContextId, result.reinvokeDelay);
                    break;

                case 7: // BREAKPOINT
                    // do nothing
                    break;
            }

            if (!this.IsEventLoopAlive && this.completionCallback != null)
            {
                this.completionCallback();
                this.completionCallback = null;
            }
        }

        private void KillEventLoop()
        {
            lock (this.state)
            {
                this.state.IsStillRunning = false;
                this.state.Queue.Clear();
            }
        }

        public bool IsEventLoopAlive
        {
            get
            {
                lock (this.state)
                {
                    return this.state.IsStillRunning;
                }
            }
        }

        internal bool RunSingleEventLoopIteration()
        {
            if (this.IsEventLoopAlive)
            {
                EventLoopInvocation invocation = PopItemFromQueue();
                if (invocation != null)
                {
                    RunEventLoopIteration(invocation);
                    return true;
                }
            }
            return false;
        }

        private object wakeupTaskMutex = new object();
        private TaskCompletionSource<bool> wakeupTask = null;

        private Task CreateNewWakeupTask()
        {
            lock (this.wakeupTaskMutex)
            {
                this.wakeupTask = new TaskCompletionSource<bool>();
                return this.wakeupTask.Task;
            }
        }

        public async Task RunEventLoop()
        {
            while (this.IsEventLoopAlive)
            {
                Task wakeUp = this.CreateNewWakeupTask();
                while (this.RunSingleEventLoopIteration()) { }
                if (!this.IsEventLoopAlive) return;
                await wakeUp;
            }
        }
    }
}
