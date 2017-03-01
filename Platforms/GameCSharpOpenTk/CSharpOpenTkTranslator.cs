using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pastel.Nodes;

namespace GameCSharpOpenTk
{
    public class CSharpOpenTkTranslator : LangCSharp.CSharpTranslator
    {
        public CSharpOpenTkTranslator(Platform.AbstractPlatform platform)
            : base(platform)
        {

        }

        public override void TranslateInvokeDynamicLibraryFunction(StringBuilder sb, Expression functionId, Expression argsArray)
        {
            throw new NotImplementedException();
        }
    }
}
