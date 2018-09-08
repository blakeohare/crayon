using Parser;
using Parser.ParseTree;

namespace Exporter.ByteCode.Nodes
{
    internal static class ClassReferenceEncoder
    {
        public static void Compile(ParserContext parser, ByteBuffer buffer, ClassReferenceLiteral classRef, bool outputUsed)
        {
            if (!outputUsed) throw new ParserException(classRef, "This class reference expression does nothing.");
            buffer.Add(classRef.FirstToken, OpCode.LITERAL, parser.GetClassRefConstant(classRef.ClassDefinition));
        }
    }
}
