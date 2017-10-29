namespace Crayon.ParseTree
{
    public interface IConstantValue
    {
        Expression CloneValue(Token token, TopLevelConstruct owner);
    }
}
