using Parser;
using Parser.ParseTree;
using System.Collections.Generic;

namespace Exporter.ByteCode.Nodes
{
    internal static class LambdaEncoder
    {
        public static void Compile(ByteCodeCompiler bcc, ParserContext parser, ByteBuffer buffer, Lambda lambda, bool outputUsed)
        {
            ByteCodeCompiler.EnsureUsed(lambda, outputUsed);

            ByteBuffer tBuffer = new ByteBuffer();

            List<int> offsetsForOptionalArgs = new List<int>();
            Expression[] argDefaultValues_allRequired = new Expression[lambda.Args.Length];
            FunctionDefinitionEncoder.CompileFunctionArgs(bcc, parser, tBuffer, lambda.Args, argDefaultValues_allRequired, offsetsForOptionalArgs, lambda.ClosureIds);

            bcc.Compile(parser, tBuffer, lambda.Code);

            List<int> args = new List<int>()
            {
                lambda.Args.Length, // min number of args required
                lambda.Args.Length, // max number of args supplied
                lambda.LocalScopeSize,
                tBuffer.Size,
                offsetsForOptionalArgs.Count
            };
            args.AddRange(offsetsForOptionalArgs);

            VariableId[] closureIds = lambda.ClosureIds;
            args.Add(closureIds.Length);
            foreach (VariableId closureVarId in closureIds)
            {
                args.Add(closureVarId.ClosureID);
            }

            buffer.Add(
                lambda.FirstToken,
                OpCode.LAMBDA,
                args.ToArray());

            buffer.Concat(tBuffer);
        }
    }
}
