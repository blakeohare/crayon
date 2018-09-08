using Parser;
using Parser.ParseTree;

namespace Exporter.ByteCode.Nodes
{
    internal static class ConstantEncoder
    {
        public static void CompileNull(ParserContext parser, ByteBuffer buffer, NullConstant nullConstant, bool outputUsed)
        {
            if (!outputUsed) throw new ParserException(nullConstant, "This expression doesn't do anything.");
            buffer.Add(nullConstant.FirstToken, OpCode.LITERAL, parser.GetNullConstant());
        }

        public static void CompileBoolean(ParserContext parser, ByteBuffer buffer, BooleanConstant boolConstant, bool outputUsed)
        {
            if (!outputUsed) throw new ParserException(boolConstant, "This expression does nothing.");
            buffer.Add(boolConstant.FirstToken, OpCode.LITERAL, parser.GetBoolConstant(boolConstant.Value));
        }

        public static void CompileInteger(ParserContext parser, ByteBuffer buffer, IntegerConstant intConst, bool outputUsed)
        {
            if (!outputUsed) throw new ParserException(intConst, "This expression does nothing.");
            buffer.Add(intConst.FirstToken, OpCode.LITERAL, parser.GetIntConstant(intConst.Value));
        }

        public static void CompileFloat(ParserContext parser, ByteBuffer buffer, FloatConstant floatConstant, bool outputUsed)
        {
            if (!outputUsed) throw new ParserException(floatConstant, "This expression doesn't do anything.");
            buffer.Add(floatConstant.FirstToken, OpCode.LITERAL, parser.GetFloatConstant(floatConstant.Value));
        }

        public static void CompileString(ParserContext parser, ByteBuffer buffer, StringConstant stringConstant, bool outputUsed)
        {
            if (!outputUsed) throw new ParserException(stringConstant, "This expression does nothing.");
            buffer.Add(stringConstant.FirstToken, OpCode.LITERAL, parser.GetStringConstant(stringConstant.Value));
        }
    }
}
