using Builder.ParseTree;

namespace Builder.ByteCode.Nodes
{
    internal static class CoreFunctionInvocationEncoder
    {
        public static void Compile(
            ByteCodeCompiler bcc,
            ParserContext parser,
            ByteBuffer buffer,
            CoreFunctionInvocation coreFuncInvocation,
            Expression[] argsOverrideOrNull,
            Token tokenOverrideOrNull,
            bool outputUsed)
        {
            Token token = tokenOverrideOrNull ?? coreFuncInvocation.FirstToken;
            Expression[] args = argsOverrideOrNull ?? coreFuncInvocation.Args;

            if (coreFuncInvocation.FunctionId == (int)CoreFunctionID.TYPE_IS)
            {
                ByteCodeCompiler.EnsureUsed(coreFuncInvocation, outputUsed);

                bcc.CompileExpression(parser, buffer, args[0], true);
                int typeCount = args.Length - 1;
                int[] actualArgs = new int[typeCount + 3];
                actualArgs[0] = coreFuncInvocation.FunctionId;
                actualArgs[1] = 1; // output used
                actualArgs[2] = typeCount;
                for (int i = typeCount - 1; i >= 0; --i)
                {
                    IntegerConstant typeArg = args[args.Length - 1 - i] as IntegerConstant;
                    if (typeArg == null)
                    {
                        throw new ParserException(coreFuncInvocation, "typeis requires type enum values.");
                    }
                    actualArgs[3 + i] = typeArg.Value + 1;
                }
                buffer.Add(token, OpCode.CORE_FUNCTION, actualArgs);
                return;
            }

            foreach (Expression arg in args)
            {
                bcc.CompileExpression(parser, buffer, arg, true);
            }

            buffer.Add(token, OpCode.CORE_FUNCTION, coreFuncInvocation.FunctionId, outputUsed ? 1 : 0);
        }
    }
}
