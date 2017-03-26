using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform;

namespace GameJavaAwt
{
    public class JavaAppTranslator : LangJava.JavaTranslator
    {
        public JavaAppTranslator(AbstractPlatform platform)
            : base(platform)
        { }

    }
}
