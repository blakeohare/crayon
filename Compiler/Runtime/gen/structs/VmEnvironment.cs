using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Structs
{
    public class VmEnvironment
    {
        public string[] commandLineArgs;
        public bool showLibStack;
        public string stdoutPrefix;
        public string stacktracePrefix;
        public object platformEventLoop;

        public VmEnvironment(string[] commandLineArgs, bool showLibStack, string stdoutPrefix, string stacktracePrefix, object platformEventLoop)
        {
            this.commandLineArgs = commandLineArgs;
            this.showLibStack = showLibStack;
            this.stdoutPrefix = stdoutPrefix;
            this.stacktracePrefix = stacktracePrefix;
            this.platformEventLoop = platformEventLoop;
        }
    }

}