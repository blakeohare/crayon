using System;

namespace Parser
{
    internal abstract class AbstractTypeParser
    {
        public virtual AType TryParse(TokenStream tokens) { throw new NotImplementedException(); }
        public virtual AType Parse(TokenStream tokens) { throw new NotImplementedException(); }
    }
}
