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
        DICTINOARY_KEYS_TO_VALUE_LIST,
        DICTIONARY_VALUES_TO_VALUE_LIST,
        DICTIONARY_NEW,
        LIST_GET,
        LIST_NEW,
        LIST_SET,
        MATH_ARCCOS,
        MATH_ARCSIN,
        MATH_ARCTAN,
        MATH_COS,
        MATH_SIN,
        MATH_TAN,
        PRINT_STDERR,
        PRINT_STDOUT,

        LIBRARY_FUNCTION,
    }
}
