namespace Parser
{
    public class CniFunction
    {
        public CompilationScope CompilationUnit { get; private set; }
        public string Name { get; private set; }
        public int ArgCount { get; private set; }
        public int ID { get; set; }

        public CniFunction(CompilationScope compilationUnit, string name, int argCount)
        {
            this.CompilationUnit = compilationUnit;
            this.Name = name;
            this.ArgCount = argCount;
        }
    }
}
