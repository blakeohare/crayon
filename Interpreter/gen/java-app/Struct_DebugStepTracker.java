public final class DebugStepTracker {
  public int uniqueId;
  public int originatingFileId;
  public int originatingLineIndex;
  public static final DebugStepTracker[] EMPTY_ARRAY = new DebugStepTracker[0];

  public DebugStepTracker(int uniqueId, int originatingFileId, int originatingLineIndex) {
    this.uniqueId = uniqueId;
    this.originatingFileId = originatingFileId;
    this.originatingLineIndex = originatingLineIndex;
  }
}