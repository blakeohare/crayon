public final class DictImpl {
  public int size;
  public int keyType;
  public int keyClassId;
  public int[] valueType;
  public HashMap<Integer, Integer> intToIndex;
  public HashMap<String, Integer> stringToIndex;
  public ArrayList<Value> keys;
  public ArrayList<Value> values;
  public static final DictImpl[] EMPTY_ARRAY = new DictImpl[0];

  public DictImpl(int size, int keyType, int keyClassId, int[] valueType, HashMap<Integer, Integer> intToIndex, HashMap<String, Integer> stringToIndex, ArrayList<Value> keys, ArrayList<Value> values) {
    this.size = size;
    this.keyType = keyType;
    this.keyClassId = keyClassId;
    this.valueType = valueType;
    this.intToIndex = intToIndex;
    this.stringToIndex = stringToIndex;
    this.keys = keys;
    this.values = values;
  }
}