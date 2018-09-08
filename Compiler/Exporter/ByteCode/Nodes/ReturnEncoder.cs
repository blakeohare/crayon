using Parser;
using Parser.ParseTree;

namespace Exporter.ByteCode.Nodes
{
    internal static class ReturnEncoder
    {
        public static void Compile(ByteCodeCompiler bcc, ParserContext parser, ByteBuffer buffer, ReturnStatement returnStatement)
        {
            if (returnStatement.TopLevelEntity is ConstructorDefinition)
            {
                if (returnStatement.Expression != null)
                {
                    throw new ParserException(returnStatement, "Cannot return a value from a constructor.");
                }
            }

            if (returnStatement.Expression == null || returnStatement.Expression is NullConstant)
            {
                buffer.Add(returnStatement.FirstToken, OpCode.RETURN, 0);
            }
            else
            {
                bcc.CompileExpression(parser, buffer, returnStatement.Expression, true);
                buffer.Add(returnStatement.FirstToken, OpCode.RETURN, 1);
            }
        }

    }
}
