using Parser;
using Parser.ParseTree;
using System;

namespace Exporter.ByteCode.Nodes
{
    internal static class IncrementEncoder
    {
        public static void Compile(ByteCodeCompiler bcc, ParserContext parser, ByteBuffer buffer, Increment increment, bool outputUsed)
        {
            if (!outputUsed)
            {
                throw new Exception("This should have been optimized into a += or -=");
            }

            if (increment.Root is Variable)
            {
                // OpCode re-use be damned. This should be not one, but two top-level op codes.
                // INCREMENT_INLINE and INCREMENT_POP (depending on whether outputUsed is true)
                // In fact, the code here in its current form is actually WRONG because someString++ will have
                // a '1' appended to it when it really should be an error if the variable is not an integer.
                // Same for the others below. Ideally the DUPLICATE_STACK_TOP op should be removed.
                Variable variable = (Variable)increment.Root;
                VariableId varId = variable.LocalScopeId;
                bool isClosureVar = varId.UsedByClosure;
                int scopeId = isClosureVar ? varId.ClosureID : varId.ID;
                bcc.CompileExpression(parser, buffer, increment.Root, true);
                if (increment.IsPrefix)
                {
                    buffer.Add(increment.IncrementToken, OpCode.LITERAL, parser.GetIntConstant(1));
                    buffer.Add(increment.IncrementToken, OpCode.BINARY_OP, increment.IsIncrement ? (int)Ops.ADDITION : (int)Ops.SUBTRACTION);
                    buffer.Add(increment.IncrementToken, OpCode.DUPLICATE_STACK_TOP, 1);
                    buffer.Add(variable.FirstToken, isClosureVar ? OpCode.ASSIGN_CLOSURE : OpCode.ASSIGN_LOCAL, scopeId);
                }
                else
                {
                    buffer.Add(increment.IncrementToken, OpCode.DUPLICATE_STACK_TOP, 1);
                    buffer.Add(increment.IncrementToken, OpCode.LITERAL, parser.GetIntConstant(1));
                    buffer.Add(increment.IncrementToken, OpCode.BINARY_OP, increment.IsIncrement ? (int)Ops.ADDITION : (int)Ops.SUBTRACTION);
                    buffer.Add(variable.FirstToken, isClosureVar ? OpCode.ASSIGN_CLOSURE : OpCode.ASSIGN_LOCAL, scopeId);
                }
            }
            else if (increment.Root is BracketIndex)
            {
                BracketIndex bracketIndex = (BracketIndex)increment.Root;
                bcc.CompileExpression(parser, buffer, bracketIndex.Root, true);
                bcc.CompileExpression(parser, buffer, bracketIndex.Index, true);
                buffer.Add(increment.IncrementToken, OpCode.DUPLICATE_STACK_TOP, 2);
                buffer.Add(bracketIndex.BracketToken, OpCode.INDEX);
                if (increment.IsPrefix)
                {
                    buffer.Add(increment.IncrementToken, OpCode.LITERAL, parser.GetIntConstant(1));
                    buffer.Add(increment.IncrementToken, OpCode.BINARY_OP, increment.IsIncrement ? (int)Ops.ADDITION : (int)Ops.SUBTRACTION);
                    buffer.Add(increment.IncrementToken, OpCode.ASSIGN_INDEX, 1);
                }
                else
                {
                    buffer.Add(increment.IncrementToken, OpCode.STACK_INSERTION_FOR_INCREMENT);
                    buffer.Add(increment.IncrementToken, OpCode.LITERAL, parser.GetIntConstant(1));
                    buffer.Add(increment.IncrementToken, OpCode.BINARY_OP, increment.IsIncrement ? (int)Ops.ADDITION : (int)Ops.SUBTRACTION);
                    buffer.Add(increment.IncrementToken, OpCode.ASSIGN_INDEX, 0);
                }
            }
            else if (increment.Root is DotField)
            {
                DotField dotStep = (DotField)increment.Root;
                bcc.CompileExpression(parser, buffer, dotStep.Root, true);
                buffer.Add(increment.IncrementToken, OpCode.DUPLICATE_STACK_TOP, 1);
                int nameId = parser.GetId(dotStep.StepToken.Value);
                int localeScopedNameId = nameId * parser.GetLocaleCount() + parser.GetLocaleId(dotStep.Owner.FileScope.CompilationScope.Locale);
                buffer.Add(dotStep.DotToken, OpCode.DEREF_DOT, nameId, localeScopedNameId);
                if (increment.IsPrefix)
                {
                    buffer.Add(increment.IncrementToken, OpCode.LITERAL, parser.GetIntConstant(1));
                    buffer.Add(increment.IncrementToken, OpCode.BINARY_OP, increment.IsIncrement ? (int)Ops.ADDITION : (int)Ops.SUBTRACTION);
                    buffer.Add(increment.IncrementToken, OpCode.ASSIGN_STEP, nameId, 1, localeScopedNameId);
                }
                else
                {
                    buffer.Add(increment.IncrementToken, OpCode.DUPLICATE_STACK_TOP, 2);
                    buffer.Add(increment.IncrementToken, OpCode.LITERAL, parser.GetIntConstant(1));
                    buffer.Add(increment.IncrementToken, OpCode.BINARY_OP, increment.IsIncrement ? (int)Ops.ADDITION : (int)Ops.SUBTRACTION);
                    buffer.Add(increment.IncrementToken, OpCode.ASSIGN_STEP, nameId, 0, localeScopedNameId);
                    buffer.Add(increment.IncrementToken, OpCode.STACK_SWAP_POP);
                }
            }
            else if (increment.Root is FieldReference)
            {
                FieldReference fr = (FieldReference)increment.Root;
                bool isStatic = fr.Field.IsStaticField;
                ClassDefinition cd = (ClassDefinition)fr.Field.Owner;
                int memberId = isStatic ? fr.Field.StaticMemberID : fr.Field.MemberID;

                bcc.CompileExpression(parser, buffer, fr, true);
                if (increment.IsPrefix)
                {
                    buffer.Add(increment.IncrementToken, OpCode.LITERAL, parser.GetIntConstant(1));
                    buffer.Add(increment.IncrementToken, OpCode.BINARY_OP, increment.IsIncrement ? (int)Ops.ADDITION : (int)Ops.SUBTRACTION);
                    buffer.Add(increment.IncrementToken, OpCode.DUPLICATE_STACK_TOP, 1);
                }
                else
                {
                    buffer.Add(increment.IncrementToken, OpCode.DUPLICATE_STACK_TOP, 1);
                    buffer.Add(increment.IncrementToken, OpCode.LITERAL, parser.GetIntConstant(1));
                    buffer.Add(increment.IncrementToken, OpCode.BINARY_OP, increment.IsIncrement ? (int)Ops.ADDITION : (int)Ops.SUBTRACTION);
                }
                Token token = increment.IsPrefix ? increment.FirstToken : fr.FirstToken;
                if (isStatic)
                {
                    buffer.Add(token, OpCode.ASSIGN_STATIC_FIELD, ((ClassDefinition)fr.Field.Owner).ClassID, memberId);
                }
                else
                {
                    buffer.Add(token, OpCode.ASSIGN_THIS_STEP, memberId);
                }
            }
            else
            {
                throw new ParserException(increment.IncrementToken, "Cannot apply " + (increment.IsIncrement ? "++" : "--") + " to this sort of expression.");
            }
        }
    }
}
