public class InterpreterResult
{
    public int status;
    public string errorMessage;
    public double reinvokeDelay;
    public int executionContextId;
    public bool isRootContext;
    public string loadAssemblyInformation;

    public InterpreterResult(int status, string errorMessage, double reinvokeDelay, int executionContextId, bool isRootContext, string loadAssemblyInformation)
    {
        this.status = status;
        this.errorMessage = errorMessage;
        this.reinvokeDelay = reinvokeDelay;
        this.executionContextId = executionContextId;
        this.isRootContext = isRootContext;
        this.loadAssemblyInformation = loadAssemblyInformation;
    }
}
