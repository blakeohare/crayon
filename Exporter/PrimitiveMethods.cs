namespace Crayon
{
    internal enum PrimitiveMethods
    {
        STRING_CONTAINS = 1,
        STRING_ENDSWITH,
        STRING_INDEXOF,
        STRING_LOWER,
        STRING_REPLACE,
        STRING_REVERSE,
        STRING_SPLIT,
        STRING_STARTSWITH,
        STRING_TRIM,
        STRING_UPPER,

        LIST_ADD,
        LIST_CHOICE,
        LIST_CLEAR,
        LIST_CLONE,
        LIST_CONCAT,
        LIST_CONTAINS,
        LIST_INSERT,
        LIST_JOIN,
        LIST_POP,
        LIST_REMOVE,
        LIST_REVERSE,
        LIST_SHUFFLE,
        LIST_SORT,

        DICTIONARY_CLEAR,
        DICTIONARY_CLONE,
        DICTIONARY_CONTAINS,
        DICTIONARY_GET,
        DICTIONARY_KEYS,
        DICTIONARY_REMOVE,
        DICTIONARY_VALUES,

        MAX_PRIMITIVE_VALUE, // highest value + 1
    }
}
