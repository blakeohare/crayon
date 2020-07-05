using Parser.ParseTree;

namespace Parser.ByteCode.Nodes
{
    // Non-invoked function references.
    internal static class FunctionReferenceEncoder
    {
        public static void Compile(ParserContext parser, ByteBuffer buffer, FunctionReference funcRef, bool outputUsed)
        {
            ByteCodeCompiler.EnsureUsed(funcRef, outputUsed);

            FunctionDefinition funcDef = funcRef.FunctionDefinition;

            int classIdStaticCheck = 0;
            int type = 0;
            if (funcDef.Owner is ClassDefinition)
            {
                if (funcDef.Modifiers.HasStatic)
                {
                    classIdStaticCheck = ((ClassDefinition)funcDef.Owner).ClassID;
                    type = 2;
                }
                else
                {
                    type = 1;
                }
            }
            buffer.Add(funcRef.FirstToken, OpCode.PUSH_FUNC_REF,
                funcDef.FunctionID,
                type,
                classIdStaticCheck);
        }
    }
}
