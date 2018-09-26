using Parser;
using Parser.ParseTree;
using System;
using System.Collections.Generic;

namespace Exporter.ByteCode.Nodes
{
    internal static class CastEncoder
    {
        internal static void EncodeTypeInfoToIntBuffer(List<int> buffer, ResolvedType type, bool convertNumType)
        {
            switch (type.Category)
            {
                case ResolvedTypeCategory.INTEGER: buffer.Add((int)Types.INTEGER); buffer.Add(convertNumType ? 1 : 0); break;
                case ResolvedTypeCategory.FLOAT: buffer.Add((int)Types.FLOAT); buffer.Add(convertNumType ? 1 : 0); break;
                case ResolvedTypeCategory.STRING: buffer.Add((int)Types.STRING); break;
                case ResolvedTypeCategory.BOOLEAN: buffer.Add((int)Types.BOOLEAN); break;

                case ResolvedTypeCategory.LIST:
                    buffer.Add((int)Types.LIST);
                    EncodeTypeInfoToIntBuffer(buffer, type.ListItemType, false);
                    break;

                case ResolvedTypeCategory.DICTIONARY:
                    buffer.Add((int)Types.DICTIONARY);
                    EncodeTypeInfoToIntBuffer(buffer, type.DictionaryKeyType, false);
                    EncodeTypeInfoToIntBuffer(buffer, type.DictionaryValueType, false);
                    break;

                case ResolvedTypeCategory.INSTANCE:
                    buffer.Add((int)Types.INSTANCE);
                    buffer.Add(type.ClassTypeOrReference.ClassID);
                    break;

                // no-op
                case ResolvedTypeCategory.ANY:
                case ResolvedTypeCategory.OBJECT:
                    buffer.Add((int)Types.NULL); // sentinel value for object type. This is needed for things like List<object>.
                    return;

                case ResolvedTypeCategory.VOID:
                case ResolvedTypeCategory.NULL:
                    throw new Exception(); // invalid
            }
        }

        public static void Compile(ByteCodeCompiler bcc, ParserContext parser, ByteBuffer buffer, Cast castExpression, bool outputUsed)
        {
            if (!outputUsed) throw new ParserException(castExpression, "The output of a cast must be used.");

            // This should have been filtered out before now.
            if (castExpression.Expression.ResolvedType == castExpression.ResolvedType) throw new Exception();

            bcc.CompileExpression(parser, buffer, castExpression.Expression, true);

            List<int> args = new List<int>();
            EncodeTypeInfoToIntBuffer(args, castExpression.ResolvedType, castExpression.DoIntFloatConversions);
            buffer.Add(castExpression.FirstToken, OpCode.CAST, args.ToArray());
        }
    }
}
