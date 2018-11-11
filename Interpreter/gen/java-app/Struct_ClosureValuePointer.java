public final class ClosureValuePointer {
  public Value value;
  public static final ClosureValuePointer[] EMPTY_ARRAY = new ClosureValuePointer[0];

  public ClosureValuePointer(Value value) {
    this.value = value;
  }
}