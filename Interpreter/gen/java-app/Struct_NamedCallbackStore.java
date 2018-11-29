public final class NamedCallbackStore {
  public ArrayList<java.lang.reflect.Method> callbacksById;
  public HashMap<String, HashMap<String, Integer>> callbackIdLookup;
  public static final NamedCallbackStore[] EMPTY_ARRAY = new NamedCallbackStore[0];

  public NamedCallbackStore(ArrayList<java.lang.reflect.Method> callbacksById, HashMap<String, HashMap<String, Integer>> callbackIdLookup) {
    this.callbacksById = callbacksById;
    this.callbackIdLookup = callbackIdLookup;
  }
}