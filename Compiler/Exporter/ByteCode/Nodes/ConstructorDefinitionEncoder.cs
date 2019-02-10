using Common;
using Parser;
using Parser.ParseTree;
using System.Collections.Generic;

namespace Exporter.ByteCode.Nodes
{
    internal static class ConstructorDefinitionEncoder
    {
        public static void Compile(ByteCodeCompiler bcc, ParserContext parser, ByteBuffer buffer, ConstructorDefinition constructor, ByteBuffer complexFieldInitializers)
        {
            TODO.ThrowErrorIfKeywordThisIsUsedInBaseArgsOrDefaultArgsAnywhereInConstructor();

            ByteBuffer tBuffer = new ByteBuffer();

            ClassDefinition cd = (ClassDefinition)constructor.Owner;

            List<int> offsetsForOptionalArgs = new List<int>();
            FunctionDefinitionEncoder.CompileFunctionArgs(bcc, parser, tBuffer, constructor.ArgNames, constructor.DefaultValues, offsetsForOptionalArgs, constructor.ArgLocalIds);

            int minArgs = 0;
            int maxArgs = constructor.ArgNames.Length;
            for (int i = 0; i < constructor.ArgNames.Length; ++i)
            {
                if (constructor.DefaultValues[i] == null)
                {
                    minArgs++;
                }
                else
                {
                    break;
                }
            }

            if (constructor.BaseToken != null)
            {
                bcc.CompileExpressionList(parser, tBuffer, constructor.BaseArgs, true);
                tBuffer.Add(
                    constructor.BaseToken,
                    OpCode.CALL_FUNCTION,
                    (int)FunctionInvocationType.BASE_CONSTRUCTOR,
                    constructor.BaseArgs.Length,
                    cd.BaseClass.Constructor.FunctionID,
                    0,
                    cd.BaseClass.ClassID);
            }

            if (complexFieldInitializers != null)
            {
                tBuffer.Concat(complexFieldInitializers);
            }

            bcc.Compile(parser, tBuffer, constructor.Code);
            tBuffer.Add(null, OpCode.RETURN, 0);

            List<int> args = new List<int>()
            {
                constructor.FunctionID,
                -1,
                minArgs,
                maxArgs,
                constructor.Modifiers.HasStatic ? 4 : 3,
                cd.ClassID,
                constructor.LocalScopeSize,
                tBuffer.Size,
                offsetsForOptionalArgs.Count,
            };

            args.AddRange(offsetsForOptionalArgs);

            buffer.Add(constructor.FirstToken, OpCode.FUNCTION_DEFINITION, "<constructor>", args.ToArray());
            buffer.Concat(tBuffer);
        }
    }
}
