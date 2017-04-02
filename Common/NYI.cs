using System;

namespace Common
{
    public static class NYI
    {
        public static Exception JavaListOfArraysConvertToArray() { return Throw(); }

        // Yup, it pretends to return an exception but actually throws it. This is so that you can choose whether or not to
        // use "throw" in the calling code. Regardless you'll always want to throw, but depending on the calling site, you may
        // want to do one or the other to avoid certain kinds of compiler warnings/errors e.g. unreachable code warnings, unset 
        // variable, etc.
        private static Exception Throw() { throw new System.NotImplementedException(); }
    }
}
