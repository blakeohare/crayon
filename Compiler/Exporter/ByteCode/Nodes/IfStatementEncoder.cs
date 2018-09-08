using Parser;
using Parser.ParseTree;

namespace Exporter.ByteCode.Nodes
{
    internal static class IfStatementEncoder
    {
        public static void Compile(ByteCodeCompiler bcc, ParserContext parser, ByteBuffer buffer, IfStatement ifStatement)
        {
            bcc.CompileExpression(parser, buffer, ifStatement.Condition, true);
            ByteBuffer trueCode = new ByteBuffer();
            bcc.Compile(parser, trueCode, ifStatement.TrueCode);
            ByteBuffer falseCode = new ByteBuffer();
            bcc.Compile(parser, falseCode, ifStatement.FalseCode);

            if (falseCode.Size == 0)
            {
                if (trueCode.Size == 0) buffer.Add(ifStatement.Condition.FirstToken, OpCode.POP);
                else
                {
                    buffer.Add(ifStatement.Condition.FirstToken, OpCode.JUMP_IF_FALSE, trueCode.Size);
                    buffer.Concat(trueCode);
                }
            }
            else
            {
                trueCode.Add(null, OpCode.JUMP, falseCode.Size);
                buffer.Add(ifStatement.Condition.FirstToken, OpCode.JUMP_IF_FALSE, trueCode.Size);
                buffer.Concat(trueCode);
                buffer.Concat(falseCode);
            }
        }
    }
}
