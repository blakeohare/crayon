using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Structs
{
    public class MagicNumbers
    {
        public int coreExceptionClassId;
        public int coreGenerateExceptionFunctionId;
        public int totalLocaleCount;

        public MagicNumbers(int coreExceptionClassId, int coreGenerateExceptionFunctionId, int totalLocaleCount)
        {
            this.coreExceptionClassId = coreExceptionClassId;
            this.coreGenerateExceptionFunctionId = coreGenerateExceptionFunctionId;
            this.totalLocaleCount = totalLocaleCount;
        }
    }

}
