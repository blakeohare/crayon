public class VmEnvironment
{
    public string[] commandLineArgs;
    public bool showLibStack;
    public string stdoutPrefix;
    public string stacktracePrefix;

    public VmEnvironment(string[] commandLineArgs, bool showLibStack, string stdoutPrefix, string stacktracePrefix)
    {
        this.commandLineArgs = commandLineArgs;
        this.showLibStack = showLibStack;
        this.stdoutPrefix = stdoutPrefix;
        this.stacktracePrefix = stacktracePrefix;
    }
}
