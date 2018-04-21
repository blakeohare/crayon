using Common;
using Pastel.Nodes;
using System;
using System.Collections.Generic;

namespace Platform
{
    public abstract class AbstractContextFreePlatform
    {
        public AbstractTranslator Translator { get; protected set; }
    }
}
