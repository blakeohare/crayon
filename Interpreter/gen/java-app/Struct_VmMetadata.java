public final class VmMetadata {
  public String[] identifiers;
  public ArrayList<String> identifiersBuilder;
  public HashMap<String, Integer> invIdentifiers;
  public Value[] literalTable;
  public ArrayList<Value> literalTableBuilder;
  public HashMap<Integer, Integer>[] integerSwitchLookups;
  public ArrayList<HashMap<Integer, Integer>> integerSwitchLookupsBuilder;
  public HashMap<String, Integer>[] stringSwitchLookups;
  public ArrayList<HashMap<String, Integer>> stringSwitchLookupsBuilder;
  public ClassInfo[] classTable;
  public FunctionInfo[] functionTable;
  public HashMap<Integer, FunctionInfo> lambdaTable;
  public int[] globalNameIdToPrimitiveMethodName;
  public HashMap<Integer, java.lang.reflect.Method> cniFunctionsById;
  public int lengthId;
  public int[] primitiveMethodFunctionIdFallbackLookup;
  public int userCodeStart;
  public String projectId;
  public int[][] esfData;
  public MagicNumbers magicNumbers;
  public HashMap<String, Integer> invFunctionNameLiterals;
  public HashMap<Integer, HashMap<Integer, Integer>> classMemberLocalizerBuilder;
  public FunctionInfo mostRecentFunctionDef;
  public static final VmMetadata[] EMPTY_ARRAY = new VmMetadata[0];

  public VmMetadata(String[] identifiers, ArrayList<String> identifiersBuilder, HashMap<String, Integer> invIdentifiers, Value[] literalTable, ArrayList<Value> literalTableBuilder, HashMap<Integer, Integer>[] integerSwitchLookups, ArrayList<HashMap<Integer, Integer>> integerSwitchLookupsBuilder, HashMap<String, Integer>[] stringSwitchLookups, ArrayList<HashMap<String, Integer>> stringSwitchLookupsBuilder, ClassInfo[] classTable, FunctionInfo[] functionTable, HashMap<Integer, FunctionInfo> lambdaTable, int[] globalNameIdToPrimitiveMethodName, HashMap<Integer, java.lang.reflect.Method> cniFunctionsById, int lengthId, int[] primitiveMethodFunctionIdFallbackLookup, int userCodeStart, String projectId, int[][] esfData, MagicNumbers magicNumbers, HashMap<String, Integer> invFunctionNameLiterals, HashMap<Integer, HashMap<Integer, Integer>> classMemberLocalizerBuilder, FunctionInfo mostRecentFunctionDef) {
    this.identifiers = identifiers;
    this.identifiersBuilder = identifiersBuilder;
    this.invIdentifiers = invIdentifiers;
    this.literalTable = literalTable;
    this.literalTableBuilder = literalTableBuilder;
    this.integerSwitchLookups = integerSwitchLookups;
    this.integerSwitchLookupsBuilder = integerSwitchLookupsBuilder;
    this.stringSwitchLookups = stringSwitchLookups;
    this.stringSwitchLookupsBuilder = stringSwitchLookupsBuilder;
    this.classTable = classTable;
    this.functionTable = functionTable;
    this.lambdaTable = lambdaTable;
    this.globalNameIdToPrimitiveMethodName = globalNameIdToPrimitiveMethodName;
    this.cniFunctionsById = cniFunctionsById;
    this.lengthId = lengthId;
    this.primitiveMethodFunctionIdFallbackLookup = primitiveMethodFunctionIdFallbackLookup;
    this.userCodeStart = userCodeStart;
    this.projectId = projectId;
    this.esfData = esfData;
    this.magicNumbers = magicNumbers;
    this.invFunctionNameLiterals = invFunctionNameLiterals;
    this.classMemberLocalizerBuilder = classMemberLocalizerBuilder;
    this.mostRecentFunctionDef = mostRecentFunctionDef;
  }
}