namespace Parser.ParseTree
{
    public class CompileTimeDictionary : Expression
    {
        public override bool CanAssignTo { get { return false; } }

        public string Type { get; private set; }

        public CompileTimeDictionary(Token firstToken, string type, Node owner)
            : base(firstToken, owner)
        {
            this.Type = type;
        }

        internal override Expression Resolve(ParserContext parser)
        {
            return this;
        }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            return this;
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase) { }
    }
}
