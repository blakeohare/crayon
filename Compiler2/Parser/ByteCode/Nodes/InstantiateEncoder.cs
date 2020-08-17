using Parser.ParseTree;

namespace Parser.ByteCode.Nodes
{
    internal static class InstantiateEncoder
    {
        public static void Compile(ByteCodeCompiler bcc, ParserContext parser, ByteBuffer buffer, Instantiate instantiate, bool outputUsed)
        {
            ClassDefinition cd = instantiate.Class;
            ConstructorDefinition constructor = cd.Constructor;

            bcc.CompileExpressionList(parser, buffer, instantiate.Args, true);
            buffer.Add(instantiate.NameToken,
                OpCode.CALL_FUNCTION,
                (int)FunctionInvocationType.CONSTRUCTOR,
                instantiate.Args.Length,
                constructor.FunctionID,
                outputUsed ? 1 : 0,
                cd.ClassID);
        }
    }
}
