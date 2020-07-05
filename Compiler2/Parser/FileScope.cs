using Parser.ParseTree;
using System.Collections.Generic;

namespace Parser
{
    internal class FileScope
    {
        public string Name { get; private set; }
        public int ID { get; private set; }
        internal string Content { get; private set; }
        internal HashSet<ImportStatement> Imports { get; private set; }
        public CompilationScope CompilationScope { get; private set; }

        internal FileScopedEntityLookup FileScopeEntityLookup { get; private set; }

        public FileScope(string filename, string content, CompilationScope scope, int id)
        {
            this.Name = filename;
            this.ID = id;
            this.Content = content;
            this.Imports = new HashSet<ImportStatement>();
            this.FileScopeEntityLookup = new FileScopedEntityLookup().SetFileScope(this);
            this.CompilationScope = scope;
        }

        internal ClassDefinition DoClassLookup(Node fromWhere, Token nameToken, string name)
        {
            return DoClassLookup(fromWhere, nameToken, name, false);
        }

        internal ClassDefinition DoClassLookup(Node fromWhere, Token nameToken, string name, bool failSilently)
        {
            TopLevelEntity ex = this.FileScopeEntityLookup.DoEntityLookup(name, fromWhere);
            if (ex == null)
            {
                if (failSilently)
                {
                    return null;
                }

                string message = "No class named '" + name + "' was found.";
                if (name.Contains("."))
                {
                    message += " Did you forget to import a library?";
                }
                throw new ParserException(nameToken, message);
            }

            if (ex is ClassDefinition)
            {
                return (ClassDefinition)ex;
            }

            // Still throw an exception if the found item is not a class. This is used by code to check if
            // something is a valid variable name or a class name. Colliding with something else is bad.
            throw new ParserException(nameToken, "This is not a class.");
        }

        public override string ToString()
        {
            return "FileScope: " + this.Name;
        }
    }
}
