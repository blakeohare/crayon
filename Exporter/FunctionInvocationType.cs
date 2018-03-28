namespace Exporter
{
    // duplicated in Constants.cry
    internal enum FunctionInvocationType
    {
        NORMAL_FUNCTION = 1,
        STATIC_METHOD = 2,
        LOCAL_METHOD = 3,
        POINTER_PROVIDED = 4,
        FIELD_INVOCATION = 5,
        CONSTRUCTOR = 6,
        BASE_CONSTRUCTOR = 7,
        STATIC_CONSTRUCTOR = 8,
        PRIMITIVE_METHOD = 9,
    }
}
