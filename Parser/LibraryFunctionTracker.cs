using System.Collections.Generic;

namespace Parser
{
    public class LibraryFunctionTracker
    {
        public Dictionary<string, string> FunctionNameToLibraryName { get; private set; }
        public Dictionary<string, int> LibFunctionIds { get; private set; }
        public List<string> OrderedListOfFunctionNames { get; private set; }

        public LibraryFunctionTracker()
        {
            this.FunctionNameToLibraryName = new Dictionary<string, string>();
            this.LibFunctionIds = new Dictionary<string, int>();
            this.OrderedListOfFunctionNames = new List<string>();
        }

        public int GetIdForFunction(string name, string library)
        {
            if (this.LibFunctionIds.ContainsKey(name))
            {
                return this.LibFunctionIds[name];
            }

            this.FunctionNameToLibraryName[name] = library;
            this.OrderedListOfFunctionNames.Add(name);
            int id = this.OrderedListOfFunctionNames.Count;
            this.LibFunctionIds[name] = id;
            return id;
        }

    }
}
