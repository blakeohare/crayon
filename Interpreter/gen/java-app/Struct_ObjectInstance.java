public final class ObjectInstance {
  public int classId;
  public int objectId;
  public Value[] members;
  public Object[] nativeData;
  public Object nativeObject;
  public static final ObjectInstance[] EMPTY_ARRAY = new ObjectInstance[0];

  public ObjectInstance(int classId, int objectId, Value[] members, Object[] nativeData, Object nativeObject) {
    this.classId = classId;
    this.objectId = objectId;
    this.members = members;
    this.nativeData = nativeData;
    this.nativeObject = nativeObject;
  }
}