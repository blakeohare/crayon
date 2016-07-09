namespace Crayon.ParseTree
{
    internal interface IConstantValue
    {
        Expression CloneValue(Token token, Executable owner);
    }
}
