public final class VmEnvironment {
  public String[] commandLineArgs;
  public boolean showLibStack;
  public String stdoutPrefix;
  public String stacktracePrefix;
  public static final VmEnvironment[] EMPTY_ARRAY = new VmEnvironment[0];

  public VmEnvironment(String[] commandLineArgs, boolean showLibStack, String stdoutPrefix, String stacktracePrefix) {
    this.commandLineArgs = commandLineArgs;
    this.showLibStack = showLibStack;
    this.stdoutPrefix = stdoutPrefix;
    this.stacktracePrefix = stacktracePrefix;
  }
}