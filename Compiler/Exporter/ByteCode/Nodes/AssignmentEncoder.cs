using Parser;
using Parser.ParseTree;
using System;

namespace Exporter.ByteCode.Nodes
{
    internal static class AssignmentEncoder
    {
        public static void Compile(ByteCodeCompiler bcc, ParserContext parser, ByteBuffer buffer, Assignment assignment)
        {
            Expression target = assignment.Target;
            Expression value = assignment.Value;

            ResolvedType targetAssignmentType = target.ResolvedType;
            if (targetAssignmentType == null) throw new Exception(); // should be ANY if no type is set.

            if (value.ResolvedType == ResolvedType.ANY &&
                 targetAssignmentType != ResolvedType.ANY)
            {
                value = new Cast(value.FirstToken, targetAssignmentType, value, value.Owner, false);
            }

            if (assignment.Op == Ops.EQUALS)
            {
                if (assignment.Target is Variable)
                {
                    Variable varTarget = (Variable)assignment.Target;
                    bcc.CompileExpression(parser, buffer, value, true);
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
                    bcc.CompileExpression(parser, buffer, value, true);
                    buffer.Add(assignment.OpToken, OpCode.ASSIGN_INDEX, 0);
                }
                else if (assignment.Target is DotField)
                {
                    DotField dotStep = (DotField)assignment.Target;
                    if (dotStep.Root is ThisKeyword)
                    {
                        bcc.CompileExpression(parser, buffer, value, true);
                        buffer.Add(assignment.OpToken, OpCode.ASSIGN_THIS_FIELD, parser.GetId(dotStep.FieldToken.Value));
                    }
                    else
                    {
                        bcc.CompileExpression(parser, buffer, dotStep.Root, true);
                        bcc.CompileExpression(parser, buffer, value, true);
                        int nameId = parser.GetId(dotStep.FieldToken.Value);
                        int localeScopedNameId = nameId * parser.GetLocaleCount() + parser.GetLocaleId(dotStep.Owner.FileScope.CompilationScope.Locale);
                        buffer.Add(
                            assignment.OpToken,
                            OpCode.ASSIGN_FIELD,
                            nameId,
                            0,
                            localeScopedNameId,
                            assignment.ClassOwner == null ? -1 : assignment.ClassOwner.ClassID,
                            assignment.CompilationScope.ScopeNumId,
                            -1, 0);
                    }
                }
                else if (assignment.Target is FieldReference)
                {
                    bcc.CompileExpression(parser, buffer, value, true);
                    FieldReference fieldReference = (FieldReference)assignment.Target;
                    if (fieldReference.Field.Modifiers.HasStatic)
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
                            OpCode.ASSIGN_THIS_FIELD,
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
                    bcc.CompileExpression(parser, buffer, value, true);
                    buffer.Add(assignment.OpToken, OpCode.BINARY_OP, (int)op);
                    buffer.Add(assignment.Target.FirstToken, isClosure ? OpCode.ASSIGN_CLOSURE : OpCode.ASSIGN_LOCAL, scopeId);
                }
                else if (assignment.Target is DotField)
                {
                    DotField dotExpr = (DotField)assignment.Target;
                    int fieldId = parser.GetId(dotExpr.FieldToken.Value);
                    int localeScopedStepId = parser.GetLocaleCount() * fieldId + parser.GetLocaleId(dotExpr.Owner.FileScope.CompilationScope.Locale);
                    bcc.CompileExpression(parser, buffer, dotExpr.Root, true);
                    if (!(dotExpr.Root is ThisKeyword))
                    {
                        buffer.Add(null, OpCode.DUPLICATE_STACK_TOP, 1);
                    }
                    buffer.Add(
                        dotExpr.DotToken,
                        OpCode.DEREF_DOT,
                        fieldId,
                        localeScopedStepId,
                        assignment.ClassOwner == null ? -1 : assignment.ClassOwner.ClassID,
                        assignment.CompilationScope.ScopeNumId,
                        -1, 0);
                    bcc.CompileExpression(parser, buffer, value, true);
                    buffer.Add(assignment.OpToken, OpCode.BINARY_OP, (int)op);
                    if (dotExpr.Root is ThisKeyword)
                    {
                        buffer.Add(assignment.OpToken, OpCode.ASSIGN_THIS_FIELD, fieldId);
                    }
                    else
                    {
                        int localeScopedNameId = fieldId * parser.GetLocaleCount() + parser.GetLocaleId(dotExpr.Owner.FileScope.CompilationScope.Locale);
                        buffer.Add(
                            assignment.OpToken,
                            OpCode.ASSIGN_FIELD,
                            fieldId,
                            0,
                            localeScopedNameId,
                            assignment.ClassOwner == null ? -1 : assignment.ClassOwner.ClassID,
                            assignment.CompilationScope.ScopeNumId,
                            -1, 0);
                    }
                }
                else if (assignment.Target is BracketIndex)
                {
                    BracketIndex indexExpr = (BracketIndex)assignment.Target;
                    bcc.CompileExpression(parser, buffer, indexExpr.Root, true);
                    bcc.CompileExpression(parser, buffer, indexExpr.Index, true);
                    buffer.Add(null, OpCode.DUPLICATE_STACK_TOP, 2);
                    buffer.Add(indexExpr.BracketToken, OpCode.INDEX);
                    bcc.CompileExpression(parser, buffer, value, true);
                    buffer.Add(assignment.OpToken, OpCode.BINARY_OP, (int)op);
                    buffer.Add(assignment.OpToken, OpCode.ASSIGN_INDEX, 0);
                }
                else if (assignment.Target is FieldReference)
                {
                    FieldReference fieldRef = (FieldReference)assignment.Target;
                    FieldReferenceEncoder.Compile(parser, buffer, fieldRef, true);
                    bcc.CompileExpression(parser, buffer, value, true);
                    buffer.Add(assignment.OpToken, OpCode.BINARY_OP, (int)op);

                    if (fieldRef.Field.Modifiers.HasStatic)
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
                            OpCode.ASSIGN_THIS_FIELD,
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
