using Parser;
using Parser.ParseTree;
using System;

namespace Exporter.ByteCode.Nodes
{
    internal static class AssignmentEncoder
    {
        public static void Compile(ByteCodeCompiler bcc, ParserContext parser, ByteBuffer buffer, Assignment assignment)
        {
            if (assignment.Op == Ops.EQUALS)
            {
                if (assignment.Target is Variable)
                {
                    Variable varTarget = (Variable)assignment.Target;
                    bcc.CompileExpression(parser, buffer, assignment.Value, true);
                    VariableId varId = varTarget.LocalScopeId;
                    if (varId.UsedByClosure)
                    {
                        buffer.Add(assignment.OpToken, OpCode.ASSIGN_CLOSURE, varId.ClosureID);
                    }
                    else
                    {
                        buffer.Add(assignment.OpToken, OpCode.ASSIGN_LOCAL, varId.ID);
                    }
                }
                else if (assignment.Target is BracketIndex)
                {
                    BracketIndex bi = (BracketIndex)assignment.Target;
                    bcc.CompileExpression(parser, buffer, bi.Root, true);
                    bcc.CompileExpression(parser, buffer, bi.Index, true);
                    bcc.CompileExpression(parser, buffer, assignment.Value, true);
                    buffer.Add(assignment.OpToken, OpCode.ASSIGN_INDEX, 0);
                }
                else if (assignment.Target is DotField)
                {
                    DotField dotStep = (DotField)assignment.Target;
                    if (dotStep.Root is ThisKeyword)
                    {
                        bcc.CompileExpression(parser, buffer, assignment.Value, true);
                        buffer.Add(assignment.OpToken, OpCode.ASSIGN_THIS_STEP, parser.GetId(dotStep.StepToken.Value));
                    }
                    else
                    {
                        bcc.CompileExpression(parser, buffer, dotStep.Root, true);
                        bcc.CompileExpression(parser, buffer, assignment.Value, true);
                        int nameId = parser.GetId(dotStep.StepToken.Value);
                        int localeScopedNameId = nameId * parser.GetLocaleCount() + parser.GetLocaleId(dotStep.Owner.FileScope.CompilationScope.Locale);
                        buffer.Add(assignment.OpToken, OpCode.ASSIGN_STEP, nameId, 0, localeScopedNameId);
                    }
                }
                else if (assignment.Target is FieldReference)
                {
                    bcc.CompileExpression(parser, buffer, assignment.Value, true);
                    FieldReference fieldReference = (FieldReference)assignment.Target;
                    if (fieldReference.Field.IsStaticField)
                    {
                        buffer.Add(
                            assignment.OpToken,
                            OpCode.ASSIGN_STATIC_FIELD,
                            ((ClassDefinition)fieldReference.Field.Owner).ClassID,
                            fieldReference.Field.StaticMemberID);
                    }
                    else
                    {
                        buffer.Add(
                            assignment.OpToken,
                            OpCode.ASSIGN_THIS_STEP,
                            fieldReference.Field.MemberID);
                    }
                }
                else
                {
                    throw new Exception("This shouldn't happen.");
                }
            }
            else
            {
                Ops op = assignment.Op;
                if (assignment.Target is Variable)
                {
                    Variable varTarget = (Variable)assignment.Target;
                    VariableId varId = varTarget.LocalScopeId;
                    bool isClosure = varId.UsedByClosure;
                    int scopeId = isClosure ? varId.ClosureID : varId.ID;

                    buffer.Add(varTarget.FirstToken, isClosure ? OpCode.DEREF_CLOSURE : OpCode.LOCAL, scopeId);
                    bcc.CompileExpression(parser, buffer, assignment.Value, true);
                    buffer.Add(assignment.OpToken, OpCode.BINARY_OP, (int)op);
                    buffer.Add(assignment.Target.FirstToken, isClosure ? OpCode.ASSIGN_CLOSURE : OpCode.ASSIGN_LOCAL, scopeId);
                }
                else if (assignment.Target is DotField)
                {
                    DotField dotExpr = (DotField)assignment.Target;
                    int stepId = parser.GetId(dotExpr.StepToken.Value);
                    int localeScopedStepId = parser.GetLocaleCount() * stepId + parser.GetLocaleId(dotExpr.Owner.FileScope.CompilationScope.Locale);
                    bcc.CompileExpression(parser, buffer, dotExpr.Root, true);
                    if (!(dotExpr.Root is ThisKeyword))
                    {
                        buffer.Add(null, OpCode.DUPLICATE_STACK_TOP, 1);
                    }
                    buffer.Add(dotExpr.DotToken, OpCode.DEREF_DOT, stepId, localeScopedStepId);
                    bcc.CompileExpression(parser, buffer, assignment.Value, true);
                    buffer.Add(assignment.OpToken, OpCode.BINARY_OP, (int)op);
                    if (dotExpr.Root is ThisKeyword)
                    {
                        buffer.Add(assignment.OpToken, OpCode.ASSIGN_THIS_STEP, stepId);
                    }
                    else
                    {
                        int localeScopedNameId = stepId * parser.GetLocaleCount() + parser.GetLocaleId(dotExpr.Owner.FileScope.CompilationScope.Locale);
                        buffer.Add(assignment.OpToken, OpCode.ASSIGN_STEP, stepId, 0, localeScopedNameId);
                    }
                }
                else if (assignment.Target is BracketIndex)
                {
                    BracketIndex indexExpr = (BracketIndex)assignment.Target;
                    bcc.CompileExpression(parser, buffer, indexExpr.Root, true);
                    bcc.CompileExpression(parser, buffer, indexExpr.Index, true);
                    buffer.Add(null, OpCode.DUPLICATE_STACK_TOP, 2);
                    buffer.Add(indexExpr.BracketToken, OpCode.INDEX);
                    bcc.CompileExpression(parser, buffer, assignment.Value, true);
                    buffer.Add(assignment.OpToken, OpCode.BINARY_OP, (int)op);
                    buffer.Add(assignment.OpToken, OpCode.ASSIGN_INDEX, 0);
                }
                else if (assignment.Target is FieldReference)
                {
                    FieldReference fieldRef = (FieldReference)assignment.Target;
                    FieldReferenceEncoder.Compile(parser, buffer, fieldRef, true);
                    bcc.CompileExpression(parser, buffer, assignment.Value, true);
                    buffer.Add(assignment.OpToken, OpCode.BINARY_OP, (int)op);

                    if (fieldRef.Field.IsStaticField)
                    {
                        buffer.Add(
                            assignment.OpToken,
                            OpCode.ASSIGN_STATIC_FIELD,
                            ((ClassDefinition)fieldRef.Field.Owner).ClassID,
                            fieldRef.Field.StaticMemberID);
                    }
                    else
                    {
                        buffer.Add(
                            assignment.OpToken,
                            OpCode.ASSIGN_THIS_STEP,
                            fieldRef.Field.MemberID);
                    }
                }
                else
                {
                    throw new ParserException(assignment.OpToken, "Assignment is not allowed on this sort of expression.");
                }
            }
        }
    }
}
