using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform;

namespace GameJavaAwt
{
    public class JavaAwtTranslator : LangJava.JavaTranslator
    {
        public JavaAwtTranslator(AbstractPlatform platform)
            : base(platform)
        { }

    }
}
