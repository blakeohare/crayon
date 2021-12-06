namespace Builder.ParseTree
{
    internal interface IConstantValue
    {
        Expression CloneValue(Token token, Node owner);
    }
}
