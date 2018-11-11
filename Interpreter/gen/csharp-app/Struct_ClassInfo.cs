public class ClassInfo
{
    public int id;
    public int nameId;
    public int baseClassId;
    public int assemblyId;
    public int staticInitializationState;
    public Value[] staticFields;
    public int staticConstructorFunctionId;
    public int constructorFunctionId;
    public int memberCount;
    public int[] functionIds;
    public int[] fieldInitializationCommand;
    public Value[] fieldInitializationLiteral;
    public int[] fieldAccessModifiers;
    public Dictionary<int, int> globalIdToMemberId;
    public Dictionary<int, int> localeScopedNameIdToMemberId;
    public int[][] typeInfo;
    public string fullyQualifiedName;

    public ClassInfo(int id, int nameId, int baseClassId, int assemblyId, int staticInitializationState, Value[] staticFields, int staticConstructorFunctionId, int constructorFunctionId, int memberCount, int[] functionIds, int[] fieldInitializationCommand, Value[] fieldInitializationLiteral, int[] fieldAccessModifiers, Dictionary<int, int> globalIdToMemberId, Dictionary<int, int> localeScopedNameIdToMemberId, int[][] typeInfo, string fullyQualifiedName)
    {
        this.id = id;
        this.nameId = nameId;
        this.baseClassId = baseClassId;
        this.assemblyId = assemblyId;
        this.staticInitializationState = staticInitializationState;
        this.staticFields = staticFields;
        this.staticConstructorFunctionId = staticConstructorFunctionId;
        this.constructorFunctionId = constructorFunctionId;
        this.memberCount = memberCount;
        this.functionIds = functionIds;
        this.fieldInitializationCommand = fieldInitializationCommand;
        this.fieldInitializationLiteral = fieldInitializationLiteral;
        this.fieldAccessModifiers = fieldAccessModifiers;
        this.globalIdToMemberId = globalIdToMemberId;
        this.localeScopedNameIdToMemberId = localeScopedNameIdToMemberId;
        this.typeInfo = typeInfo;
        this.fullyQualifiedName = fullyQualifiedName;
    }
}
