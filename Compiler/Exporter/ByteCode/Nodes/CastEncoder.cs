using Parser;
using Parser.ParseTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exporter.ByteCode.Nodes
{
    internal static class CastEncoder
    {
        private static void EncodeTypeInfoToIntBuffer(List<int> buffer, ResolvedType type)
        {
            switch (type.Category)
            {
                case ResolvedTypeCategory.INTEGER: buffer.Add((int)Types.INTEGER); break;
                case ResolvedTypeCategory.FLOAT: buffer.Add((int)Types.FLOAT); break;
                case ResolvedTypeCategory.STRING: buffer.Add((int)Types.STRING); break;
                case ResolvedTypeCategory.BOOLEAN: buffer.Add((int)Types.BOOLEAN); break;

                case ResolvedTypeCategory.LIST:
                    buffer.Add((int)Types.LIST);
                    EncodeTypeInfoToIntBuffer(buffer, type.ListItemType);
                    break;

                case ResolvedTypeCategory.DICTIONARY:
                    buffer.Add((int)Types.DICTIONARY);
                    EncodeTypeInfoToIntBuffer(buffer, type.DictionaryKeyType);
                    EncodeTypeInfoToIntBuffer(buffer, type.DictionaryValueType);
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

        public static void  Compile(ByteCodeCompiler bcc, ParserContext parser, ByteBuffer buffer, Cast castExpression, bool outputUsed)
        {
            if (!outputUsed) throw new ParserException(castExpression, "The output of a cast must be used.");

            // This should have been filtered out before now.
            if (castExpression.Expression.ResolvedType == castExpression.ResolvedType) throw new Exception();

            bcc.CompileExpression(parser, buffer, castExpression.Expression, true);

            List<int> args = new List<int>();
            EncodeTypeInfoToIntBuffer(args, castExpression.ResolvedType);
            buffer.Add(castExpression.FirstToken, OpCode.CAST, args.ToArray());
        }
    }
}
