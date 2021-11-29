using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Structs
{
    public class Token
    {
        public int lineIndex;
        public int colIndex;
        public int fileId;

        public Token(int lineIndex, int colIndex, int fileId)
        {
            this.lineIndex = lineIndex;
            this.colIndex = colIndex;
            this.fileId = fileId;
        }
    }

}
