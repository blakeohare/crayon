using Parser;
using Parser.ParseTree;

namespace Exporter.ByteCode.Nodes
{
    internal static class CniFunctionInvocationEncoder
    {
        public static void Compile(
            ByteCodeCompiler bcc,
            ParserContext parser,
            ByteBuffer buffer,
            CniFunctionInvocation cniFuncInvocation,
            Expression[] argsOverrideOrNull,
            Token throwTokenOverrideOrNull,
            bool outputUsed)
        {
            CniFunction cniFunc = cniFuncInvocation.CniFunction;
            Expression[] args = argsOverrideOrNull ?? cniFuncInvocation.Args;
            foreach (Expression arg in args)
            {
                bcc.CompileExpression(parser, buffer, arg, true);
            }
            Token throwToken = throwTokenOverrideOrNull ?? cniFuncInvocation.FirstToken;
            buffer.Add(throwToken, OpCode.CNI_INVOKE, cniFunc.ID, cniFunc.ArgCount, outputUsed ? 1 : 0);
        }
    }
}
