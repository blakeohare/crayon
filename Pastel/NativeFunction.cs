using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pastel
{
    public enum NativeFunction
    {
        NONE,

        ARRAY_GET,
        ARRAY_SET,
        DICTIONARY_CONTAINS_KEY,
        DICTIONARY_KEYS,
        DICTINOARY_KEYS_TO_VALUE_LIST,
        DICTIONARY_SIZE,
        DICTIONARY_VALUES_TO_VALUE_LIST,
        DICTIONARY_NEW,
        GET_RESOURCE_MANIFEST,
        LIST_GET,
        LIST_NEW,
        LIST_SET,
        LIST_TO_ARRAY,
        MATH_ARCCOS,
        MATH_ARCSIN,
        MATH_ARCTAN,
        MATH_COS,
        MATH_SIN,
        MATH_TAN,
        PRINT_STDERR,
        PRINT_STDOUT,
        READ_BYTE_CODE_FILE,
        SET_PROGRAM_DATA,
        SORTED_COPY_OF_INT_ARRAY,
        SORTED_COPY_OF_STRING_ARRAY,
        STRING_EQUALS,
        STRING_LENGTH,

        LIBRARY_FUNCTION,
    }
}
