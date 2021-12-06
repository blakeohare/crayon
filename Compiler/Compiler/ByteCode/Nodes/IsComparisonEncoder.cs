using Builder.ParseTree;

namespace Builder.ByteCode.Nodes
{
    internal static class IsComparisonEncoder
    {
        public static void Compile(ByteCodeCompiler bcc, ParserContext parser, ByteBuffer buffer, IsComparison isComp, bool outputUsed)
        {
            ByteCodeCompiler.EnsureUsed(isComp.IsToken, outputUsed);
            bcc.CompileExpression(parser, buffer, isComp.Expression, true);
            buffer.Add(isComp.IsToken, OpCode.IS_COMPARISON, isComp.ClassDefinition.ClassID);
        }
    }
}
