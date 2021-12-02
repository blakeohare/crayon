using System.Collections.Generic;

namespace Interpreter.Structs
{
    public class Code
    {
        public int[] ops;
        public int[][] args;
        public string[] stringArgs;
        public Dictionary<int, int>[] integerSwitchesByPc;
        public Dictionary<string, int>[] stringSwitchesByPc;
        public VmDebugData debugData;

        public Code(int[] ops, int[][] args, string[] stringArgs, Dictionary<int, int>[] integerSwitchesByPc, Dictionary<string, int>[] stringSwitchesByPc, VmDebugData debugData)
        {
            this.ops = ops;
            this.args = args;
            this.stringArgs = stringArgs;
            this.integerSwitchesByPc = integerSwitchesByPc;
            this.stringSwitchesByPc = stringSwitchesByPc;
            this.debugData = debugData;
        }
    }

}
