public final class BreakpointInfo {
  public int breakpointId;
  public boolean isTransient;
  public Token token;
  public static final BreakpointInfo[] EMPTY_ARRAY = new BreakpointInfo[0];

  public BreakpointInfo(int breakpointId, boolean isTransient, Token token) {
    this.breakpointId = breakpointId;
    this.isTransient = isTransient;
    this.token = token;
  }
}