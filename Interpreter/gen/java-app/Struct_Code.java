public final class Code {
  public int[] ops;
  public int[][] args;
  public String[] stringArgs;
  public HashMap<Integer, Integer>[] integerSwitchesByPc;
  public HashMap<String, Integer>[] stringSwitchesByPc;
  public VmDebugData debugData;
  public static final Code[] EMPTY_ARRAY = new Code[0];

  public Code(int[] ops, int[][] args, String[] stringArgs, HashMap<Integer, Integer>[] integerSwitchesByPc, HashMap<String, Integer>[] stringSwitchesByPc, VmDebugData debugData) {
    this.ops = ops;
    this.args = args;
    this.stringArgs = stringArgs;
    this.integerSwitchesByPc = integerSwitchesByPc;
    this.stringSwitchesByPc = stringSwitchesByPc;
    this.debugData = debugData;
  }
}