using Parser;
using Parser.ParseTree;

namespace Exporter.ByteCode.Nodes
{
    internal static class VariableEncoder
    {
        public static void Compile(ParserContext parser, ByteBuffer buffer, Variable variable, bool outputUsed)
        {
            if (!outputUsed) throw new ParserException(variable, "This expression does nothing.");
            int nameId = parser.GetId(variable.Name);
            Token token = variable.FirstToken;
            VariableId varId = variable.LocalScopeId;
            if (varId == null)
            {
                throw new ParserException(token, "Variable used but not declared.");
            }
            bool isClosureVar = varId.UsedByClosure;
            buffer.Add(token, isClosureVar ? OpCode.DEREF_CLOSURE : OpCode.LOCAL, isClosureVar ? varId.ClosureID : varId.ID, nameId);
        }
    }
}
