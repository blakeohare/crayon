namespace Parser.ParseTree
{
    public interface IConstantValue
    {
        Expression CloneValue(Token token, Node owner);
    }
}
