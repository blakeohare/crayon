using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Structs
{
    public class SymbolData
    {
        public List<Token>[] tokenData;
        public string[] sourceCode;
        public List<string> sourceCodeBuilder;
        public string[] fileNameById;
        public Dictionary<string, int> fileIdByName;
        public Dictionary<int, List<string>> localVarNamesById;
        public Dictionary<int, List<string>> closureVarNamesById;

        public SymbolData(List<Token>[] tokenData, string[] sourceCode, List<string> sourceCodeBuilder, string[] fileNameById, Dictionary<string, int> fileIdByName, Dictionary<int, List<string>> localVarNamesById, Dictionary<int, List<string>> closureVarNamesById)
        {
            this.tokenData = tokenData;
            this.sourceCode = sourceCode;
            this.sourceCodeBuilder = sourceCodeBuilder;
            this.fileNameById = fileNameById;
            this.fileIdByName = fileIdByName;
            this.localVarNamesById = localVarNamesById;
            this.closureVarNamesById = closureVarNamesById;
        }
    }

}
