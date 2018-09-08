using Parser;
using Parser.ParseTree;

namespace Exporter.ByteCode.Nodes
{
    internal static class DotFieldEncoder
    {
        public static void Compile(ByteCodeCompiler bcc, ParserContext parser, ByteBuffer buffer, DotField dotStep, bool outputUsed)
        {
            if (!outputUsed) throw new ParserException(dotStep, "This expression does nothing.");
            bcc.CompileExpression(parser, buffer, dotStep.Root, true);
            int rawNameId = parser.GetId(dotStep.StepToken.Value);
            int localeId = parser.GetLocaleId(dotStep.Owner.FileScope.CompilationScope.Locale);
            int localeScopedNameId = rawNameId * parser.GetLocaleCount() + localeId;
            buffer.Add(dotStep.DotToken, OpCode.DEREF_DOT, rawNameId, localeScopedNameId);
        }
    }
}
