﻿using Builder.ParseTree;

namespace Builder.ByteCode.Nodes
{
    internal static class DotFieldEncoder
    {
        public static void Compile(ByteCodeCompiler bcc, ParserContext parser, ByteBuffer buffer, PrimitiveMethodReference dotField, bool outputUsed)
        {
            CompileImpl(bcc, parser, buffer, dotField.Root, dotField.DotToken, dotField.FieldToken, outputUsed);
        }

        public static void Compile(ByteCodeCompiler bcc, ParserContext parser, ByteBuffer buffer, DotField dotField, bool outputUsed)
        {
            CompileImpl(bcc, parser, buffer, dotField.Root, dotField.DotToken, dotField.FieldToken, outputUsed);
        }

        private static void CompileImpl(ByteCodeCompiler bcc, ParserContext parser, ByteBuffer buffer, Expression root, Token dotToken, Token fieldToken, bool outputUsed)
        {
            if (!outputUsed) throw new ParserException(root.FirstToken, "This expression does nothing.");
            bcc.CompileExpression(parser, buffer, root, true);
            int rawNameId = parser.GetId(fieldToken.Value);
            int localeId = parser.GetLocaleId(root.Owner.FileScope.CompilationScope.Locale);
            int localeScopedNameId = rawNameId * parser.GetLocaleCount() + localeId;
            ClassDefinition cd = root.ClassOwner;
            int classId = cd == null ? -1 : cd.ClassID;
            buffer.Add(
                dotToken,
                OpCode.DEREF_DOT,
                rawNameId,
                localeScopedNameId,
                classId,
                root.CompilationScope.ScopeNumId,
                -1, 0);
        }
    }
}
