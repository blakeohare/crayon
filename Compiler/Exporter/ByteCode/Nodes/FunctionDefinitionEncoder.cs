using Parser;
using Parser.ParseTree;
using System.Collections.Generic;
using System.Linq;

namespace Exporter.ByteCode.Nodes
{
    internal static class FunctionDefinitionEncoder
    {
        public static void Compile(
            ByteCodeCompiler bcc,
            ParserContext parser,
            ByteBuffer buffer,
            FunctionDefinition funDef,
            bool isMethod)
        {
            ByteBuffer tBuffer = new ByteBuffer();

            List<int> offsetsForOptionalArgs = new List<int>();
            CompileFunctionArgs(bcc, parser, tBuffer, funDef.ArgNames, funDef.DefaultValues, offsetsForOptionalArgs);

            if (funDef.CompilationScope.IsStaticallyTyped)
            {
                EncodeArgTypeCheck(tBuffer, funDef, funDef.ResolvedArgTypes);
            }

            bcc.Compile(parser, tBuffer, funDef.Code);

            List<int> args = new List<int>()
            {
                funDef.FunctionID,
                parser.GetId(funDef.NameToken.Value), // local var to save in
                GetMinArgCountFromDefaultValuesList(funDef.DefaultValues),
                funDef.ArgNames.Length, // max number of args supplied
                isMethod ? (funDef.Modifiers.HasStatic ? 2 : 1) : 0, // type (0 - function, 1 - method, 2 - static method)
                isMethod ? ((ClassDefinition)funDef.Owner).ClassID : 0,
                funDef.LocalScopeSize,
                tBuffer.Size,
                offsetsForOptionalArgs.Count
            };
            args.AddRange(offsetsForOptionalArgs);

            buffer.Add(
                funDef.FirstToken,
                OpCode.FUNCTION_DEFINITION,
                funDef.NameToken.Value,
                args.ToArray());

            buffer.Concat(tBuffer);
            AddDebugSymbolData(buffer, parser, funDef);
        }

        private static void AddDebugSymbolData(ByteBuffer buffer, ParserContext parser, FunctionDefinition funcDef)
        {
            if (parser.IncludeDebugSymbols)
            {
                foreach (VariableId id in funcDef.Locals.OrderBy(vid => vid.ID))
                {
                    int type = id.UsedByClosure ? 2 : 1;
                    int idNum = id.UsedByClosure ? id.ClosureID : id.ID;
                    buffer.Add(null, OpCode.DEBUG_SYMBOLS, id.Name, type, idNum);
                }
            }
        }

        internal static void CompileFunctionArgs(
            ByteCodeCompiler bcc,
            ParserContext parser,
            ByteBuffer buffer,
            IList<Token> argNames,
            IList<Expression> argValues,
            List<int> offsetsForOptionalArgs)
        {
            int bufferStartSize = buffer.Size;
            for (int i = 0; i < argNames.Count; ++i)
            {
                if (argValues[i] != null)
                {
                    bcc.CompileExpression(parser, buffer, argValues[i], true);
                    buffer.Add(argNames[i], OpCode.ASSIGN_LOCAL, i);
                    offsetsForOptionalArgs.Add(buffer.Size - bufferStartSize);
                }
            }
        }

        private static int GetMinArgCountFromDefaultValuesList(Expression[] argDefaultValues)
        {
            int minArgCount = 0;
            for (int i = 0; i < argDefaultValues.Length; ++i)
            {
                if (argDefaultValues[i] != null)
                {
                    break;
                }
                minArgCount++;
            }
            return minArgCount;
        }

        private static void EncodeArgTypeCheck(ByteBuffer byteCode, FunctionDefinition fnDef, ResolvedType[] argTypes)
        {
            if (argTypes.Length == 0 ||
                !fnDef.CompilationScope.IsStaticallyTyped ||
                argTypes.Count(t => t.Category == ResolvedTypeCategory.OBJECT) < argTypes.Length)
            {
                return;
            }

            List<int> typeVerifyArgs = new List<int>();
            int argCount = argTypes.Length;
            typeVerifyArgs.Add(argCount);
            for (int i = 0; i < argCount; ++i)
            {
                ResolvedType argType = argTypes[i];
                CastEncoder.EncodeTypeInfoToIntBuffer(typeVerifyArgs, argTypes[i], false);
            }

            byteCode.Add(fnDef.FirstToken, OpCode.ARG_TYPE_VERIFY, typeVerifyArgs.ToArray());
        }
    }
}
