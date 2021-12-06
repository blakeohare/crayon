using Builder.ParseTree;

namespace Builder.ByteCode.Nodes
{
    internal static class BaseMethodReferenceEncoder
    {
        public static void Compile(ParserContext parser, ByteBuffer buffer, BaseMethodReference baseMethodReference, bool outputUsed)
        {
            ByteCodeCompiler.EnsureUsed(baseMethodReference, outputUsed);
            int baseClassId = baseMethodReference.ClassToWhichThisMethodRefers.ClassID;
            buffer.Add(
                baseMethodReference.DotToken,
                OpCode.PUSH_FUNC_REF,
                baseMethodReference.FunctionDefinition.FunctionID,
                1, // instance method
                0);
        }
    }
}
