using Parser;
using Parser.ParseTree;

namespace Exporter.ByteCode.Nodes
{
    internal static class FieldReferenceEncoder
    {
        public static void Compile(ParserContext parser, ByteBuffer buffer, FieldReference fieldRef, bool outputUsed)
        {
            ByteCodeCompiler.EnsureUsed(fieldRef, outputUsed);

            if (fieldRef.Field.Modifiers.HasStatic)
            {
                buffer.Add(
                    fieldRef.FirstToken,
                    OpCode.DEREF_STATIC_FIELD,
                    ((ClassDefinition)fieldRef.Field.Owner).ClassID,
                    fieldRef.Field.StaticMemberID);
            }
            else
            {
                buffer.Add(
                    fieldRef.FirstToken,
                    OpCode.DEREF_INSTANCE_FIELD,
                    fieldRef.Field.MemberID);
            }
        }
    }
}
