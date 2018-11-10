namespace Pastel.Nodes
{
    internal enum CompilationEntityType
    {
        FUNCTION,
        ENUM,
        CONSTANT,
        STRUCT,
    }

    internal interface ICompilationEntity
    {
        CompilationEntityType EntityType { get; }
        PastelContext Context { get; }
    }
}
