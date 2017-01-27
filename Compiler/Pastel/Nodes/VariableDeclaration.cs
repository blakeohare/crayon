namespace Crayon.Pastel.Nodes
{
    class VariableDeclaration : Executable
    {
        public PType Type { get; set; }
        public Token VariableName { get; set; }
        public Token EqualsToken { get; set; }
        public Expression Value { get; set; }

        public VariableDeclaration(
            PType type,
            Token variableName,
            Token equalsToken,
            Expression assignmentValue) : base(type.FirstToken)
        {
            this.Type = type;
            this.VariableName = variableName;
            this.EqualsToken = equalsToken;
            this.Value = assignmentValue;
        }
    }
}
