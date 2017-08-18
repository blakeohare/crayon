using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pastel.Nodes;

namespace CApp
{
    public class CAppTranslator : LangC.CTranslator
    {
        public CAppTranslator(Platform.AbstractPlatform platform)
            : base(platform)
        { }
    }
}
