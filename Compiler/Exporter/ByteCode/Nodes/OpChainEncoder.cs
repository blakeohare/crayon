using Parser;
using Parser.ParseTree;
using System;

namespace Exporter.ByteCode.Nodes
{
    internal static class OpChainEncoder
    {
        public static void Compile(ByteCodeCompiler bcc, ParserContext parser, ByteBuffer buffer, OpChain opChain, bool outputUsed)
        {
            if (!outputUsed)
            {
                if (opChain.Op.Value == "==")
                {
                    throw new ParserException(opChain.Op, "'==' cannot be used like this. Did you mean to use just a single '=' instead?");
                }
                throw new ParserException(opChain, "This expression isn't valid here.");
            }

            bcc.CompileExpressionList(parser, buffer, new Expression[] { opChain.Left, opChain.Right }, true);

            Token opToken = opChain.Op;
            switch (opToken.Value)
            {
                case "+": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.ADDITION); break;
                case "<": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.LESS_THAN); break;
                case "<=": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.LESS_THAN_OR_EQUAL); break;
                case ">": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.GREATER_THAN); break;
                case ">=": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.GREATER_THAN_OR_EQUAL); break;
                case "-": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.SUBTRACTION); break;
                case "*": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.MULTIPLICATION); break;
                case "/": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.DIVISION); break;
                case "%": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.MODULO); break;
                case "**": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.EXPONENT); break;
                case "|": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.BITWISE_OR); break;
                case "&": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.BITWISE_AND); break;
                case "^": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.BITWISE_XOR); break;
                case "<<": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.BIT_SHIFT_LEFT); break;
                case ">>": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.BIT_SHIFT_RIGHT); break;
                case "==": buffer.Add(opToken, OpCode.EQUALS, 0); break;
                case "!=": buffer.Add(opToken, OpCode.EQUALS, 1); break;
                default: throw new NotImplementedException("Binary op: " + opChain.Op.Value);
            }

            if (!outputUsed)
            {
                buffer.Add(null, OpCode.POP);
            }
        }
    }
}
