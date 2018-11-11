public final class InterpreterResult {
  public int status;
  public String errorMessage;
  public double reinvokeDelay;
  public int executionContextId;
  public boolean isRootContext;
  public String loadAssemblyInformation;
  public static final InterpreterResult[] EMPTY_ARRAY = new InterpreterResult[0];

  public InterpreterResult(int status, String errorMessage, double reinvokeDelay, int executionContextId, boolean isRootContext, String loadAssemblyInformation) {
    this.status = status;
    this.errorMessage = errorMessage;
    this.reinvokeDelay = reinvokeDelay;
    this.executionContextId = executionContextId;
    this.isRootContext = isRootContext;
    this.loadAssemblyInformation = loadAssemblyInformation;
  }
}