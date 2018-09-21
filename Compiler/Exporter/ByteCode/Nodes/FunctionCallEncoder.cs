using Parser;
using Parser.ParseTree;
using System;
using System.Collections.Generic;

namespace Exporter.ByteCode.Nodes
{
    internal static class FunctionCallEncoder
    {
        public static void Compile(ByteCodeCompiler bcc, ParserContext parser, ByteBuffer buffer, FunctionCall funCall, bool outputUsed)
        {
            bool argCountIsNegativeOne = false;
            FunctionDefinition ownerFunction = funCall.Owner as FunctionDefinition;
            if (ownerFunction != null &&
                ownerFunction.NameToken.Value == "_LIB_CORE_invoke" &&
                ownerFunction.FileScope.CompilationScope.Dependencies.Length == 0)
            {
                argCountIsNegativeOne = true;
            }

            Expression root = funCall.Root;
            if (root is FunctionReference)
            {
                FunctionReference verifiedFunction = (FunctionReference)root;
                FunctionDefinition fd = verifiedFunction.FunctionDefinition;

                if (parser.InlinableLibraryFunctions.Contains(fd))
                {
                    CompileInlinedLibraryFunctionCall(bcc, parser, buffer, funCall, fd, outputUsed);
                }
                else
                {
                    bcc.CompileExpressionList(parser, buffer, funCall.Args, true);
                    if (fd.Owner is ClassDefinition)
                    {
                        ClassDefinition cd = (ClassDefinition)fd.Owner;
                        if (fd.IsStaticMethod)
                        {
                            buffer.Add(
                                funCall.ParenToken,
                                OpCode.CALL_FUNCTION,
                                (int)FunctionInvocationType.STATIC_METHOD,
                                funCall.Args.Length,
                                fd.FunctionID,
                                outputUsed ? 1 : 0,
                                cd.ClassID);
                        }
                        else
                        {
                            buffer.Add(
                                funCall.ParenToken,
                                OpCode.CALL_FUNCTION,
                                (int)FunctionInvocationType.LOCAL_METHOD,
                                funCall.Args.Length,
                                fd.FunctionID,
                                outputUsed ? 1 : 0,
                                cd.ClassID,
                                verifiedFunction.FunctionDefinition.MemberID);
                        }
                    }
                    else
                    {
                        // vanilla function
                        buffer.Add(
                            funCall.ParenToken,
                            OpCode.CALL_FUNCTION,
                            (int)FunctionInvocationType.NORMAL_FUNCTION,
                            funCall.Args.Length,
                            fd.FunctionID,
                            outputUsed ? 1 : 0,
                            0);
                    }
                }
            }
            else if (root is DotField)
            {
                DotField ds = (DotField)root;
                Expression dotRoot = ds.Root;
                int globalNameId = parser.GetId(ds.FieldToken.Value);
                bcc.CompileExpression(parser, buffer, dotRoot, true);
                bcc.CompileExpressionList(parser, buffer, funCall.Args, true);
                int localeId = parser.GetLocaleId(ds.Owner.FileScope.CompilationScope.Locale);
                buffer.Add(
                    funCall.ParenToken,
                    OpCode.CALL_FUNCTION,
                    (int)FunctionInvocationType.FIELD_INVOCATION,
                    funCall.Args.Length,
                    0,
                    outputUsed ? 1 : 0,
                    globalNameId,
                    localeId);
            }
            else if (root is BaseMethodReference)
            {
                BaseMethodReference bmr = (BaseMethodReference)root;
                FunctionDefinition fd = bmr.ClassToWhichThisMethodRefers.GetMethod(bmr.FieldToken.Value, true);
                if (fd == null)
                {
                    throw new ParserException(bmr.DotToken, "This method does not exist on any base class.");
                }

                bcc.CompileExpressionList(parser, buffer, funCall.Args, true);
                buffer.Add(
                    funCall.ParenToken,
                    OpCode.CALL_FUNCTION,
                    (int)FunctionInvocationType.LOCAL_METHOD,
                    funCall.Args.Length,
                    fd.FunctionID,
                    outputUsed ? 1 : 0,
                    bmr.ClassToWhichThisMethodRefers.ClassID,
                    -1);
            }
            else
            {
                bcc.CompileExpression(parser, buffer, root, true);
                bcc.CompileExpressionList(parser, buffer, funCall.Args, true);
                buffer.Add(
                    funCall.ParenToken,
                    OpCode.CALL_FUNCTION,
                    (int)FunctionInvocationType.POINTER_PROVIDED,
                    argCountIsNegativeOne ? -1 : funCall.Args.Length,
                    0,
                    outputUsed ? 1 : 0,
                    0);
            }
        }

        private static void CompileInlinedLibraryFunctionCall(
            ByteCodeCompiler bcc,
            ParserContext parser,
            ByteBuffer buffer,
            FunctionCall userWrittenOuterFunctionCall,
            FunctionDefinition embedFunctionBeingInlined,
            bool outputUsed)
        {
            Expression coreOrLibFunctionBeingInlined = ((ReturnStatement)embedFunctionBeingInlined.Code[0]).Expression;

            int embedFuncLength = embedFunctionBeingInlined.ArgNames.Length;
            int userProvidedArgLength = userWrittenOuterFunctionCall.Args.Length;
            if (userProvidedArgLength > embedFuncLength)
            {
                // TODO: can this be removed? Isn't this caught elsewhere?
                throw new ParserException(userWrittenOuterFunctionCall.ParenToken, "More arguments were passed to this function than allowed.");
            }

            // First go through the args provided and map them to the arg names of the embed function that's being inlined.
            Dictionary<string, Expression> userProvidedAndImplicitArgumentsByArgName = new Dictionary<string, Expression>();
            for (int i = 0; i < embedFuncLength; ++i)
            {
                Expression argValue;
                if (i < userProvidedArgLength)
                {
                    argValue = userWrittenOuterFunctionCall.Args[i];
                }
                else
                {
                    argValue = embedFunctionBeingInlined.DefaultValues[i];
                    if (argValue == null)
                    {
                        throw new ParserException(userWrittenOuterFunctionCall.ParenToken, "Not enough arguments were supplied to this function.");
                    }
                }
                userProvidedAndImplicitArgumentsByArgName[embedFunctionBeingInlined.ArgNames[i].Value] = argValue;
            }

            CniFunctionInvocation cniFunctionCall = coreOrLibFunctionBeingInlined as CniFunctionInvocation;
            CoreFunctionInvocation coreFunctionCall = coreOrLibFunctionBeingInlined as CoreFunctionInvocation;
            if (cniFunctionCall == null && coreFunctionCall == null)
            {
                throw new InvalidOperationException(); // This shouldn't happen. The body of the library function should have been verified by the resolver before getting to this state.
            }

            Expression[] innermostArgList = cniFunctionCall != null
                ? cniFunctionCall.Args
                : coreFunctionCall.Args;

            // This is the new list of arguments that will be passed to the inner underlying lib/core function.
            List<Expression> finalArguments = new List<Expression>();

            Expression arg;
            for (int i = 0; i < innermostArgList.Length; ++i)
            {
                arg = innermostArgList[i];
                if (arg is Variable)
                {
                    string argName = ((Variable)innermostArgList[i]).Name;
                    finalArguments.Add(userProvidedAndImplicitArgumentsByArgName[argName]);
                }
                else if (arg.IsLiteral || arg is FieldReference)
                {
                    finalArguments.Add(arg);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            if (cniFunctionCall != null)
            {
                CniFunctionInvocationEncoder.Compile(
                    bcc,
                    parser,
                    buffer,
                    cniFunctionCall,
                    finalArguments.ToArray(),
                    userWrittenOuterFunctionCall.ParenToken,
                    outputUsed);
            }
            else
            {
                CoreFunctionInvocationEncoder.Compile(
                    bcc,
                    parser,
                    buffer,
                    coreFunctionCall,
                    finalArguments.ToArray(),
                    userWrittenOuterFunctionCall.ParenToken,
                    outputUsed);
            }
        }
    }
}
