using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Structs
{
    public class VmMetadata
    {
        public string[] identifiers;
        public List<string> identifiersBuilder;
        public Dictionary<string, int> invIdentifiers;
        public Value[] literalTable;
        public List<Value> literalTableBuilder;
        public Dictionary<int, int>[] integerSwitchLookups;
        public List<Dictionary<int, int>> integerSwitchLookupsBuilder;
        public Dictionary<string, int>[] stringSwitchLookups;
        public List<Dictionary<string, int>> stringSwitchLookupsBuilder;
        public ClassInfo[] classTable;
        public FunctionInfo[] functionTable;
        public Dictionary<int, FunctionInfo> lambdaTable;
        public int[] globalNameIdToPrimitiveMethodName;
        public int lengthId;
        public int[] primitiveMethodFunctionIdFallbackLookup;
        public int userCodeStart;
        public string projectId;
        public int[][] esfData;
        public MagicNumbers magicNumbers;
        public Dictionary<string, int> invFunctionNameLiterals;
        public Dictionary<int, Dictionary<int, int>> classMemberLocalizerBuilder;
        public FunctionInfo mostRecentFunctionDef;

        public VmMetadata(string[] identifiers, List<string> identifiersBuilder, Dictionary<string, int> invIdentifiers, Value[] literalTable, List<Value> literalTableBuilder, Dictionary<int, int>[] integerSwitchLookups, List<Dictionary<int, int>> integerSwitchLookupsBuilder, Dictionary<string, int>[] stringSwitchLookups, List<Dictionary<string, int>> stringSwitchLookupsBuilder, ClassInfo[] classTable, FunctionInfo[] functionTable, Dictionary<int, FunctionInfo> lambdaTable, int[] globalNameIdToPrimitiveMethodName, int lengthId, int[] primitiveMethodFunctionIdFallbackLookup, int userCodeStart, string projectId, int[][] esfData, MagicNumbers magicNumbers, Dictionary<string, int> invFunctionNameLiterals, Dictionary<int, Dictionary<int, int>> classMemberLocalizerBuilder, FunctionInfo mostRecentFunctionDef)
        {
            this.identifiers = identifiers;
            this.identifiersBuilder = identifiersBuilder;
            this.invIdentifiers = invIdentifiers;
            this.literalTable = literalTable;
            this.literalTableBuilder = literalTableBuilder;
            this.integerSwitchLookups = integerSwitchLookups;
            this.integerSwitchLookupsBuilder = integerSwitchLookupsBuilder;
            this.stringSwitchLookups = stringSwitchLookups;
            this.stringSwitchLookupsBuilder = stringSwitchLookupsBuilder;
            this.classTable = classTable;
            this.functionTable = functionTable;
            this.lambdaTable = lambdaTable;
            this.globalNameIdToPrimitiveMethodName = globalNameIdToPrimitiveMethodName;
            this.lengthId = lengthId;
            this.primitiveMethodFunctionIdFallbackLookup = primitiveMethodFunctionIdFallbackLookup;
            this.userCodeStart = userCodeStart;
            this.projectId = projectId;
            this.esfData = esfData;
            this.magicNumbers = magicNumbers;
            this.invFunctionNameLiterals = invFunctionNameLiterals;
            this.classMemberLocalizerBuilder = classMemberLocalizerBuilder;
            this.mostRecentFunctionDef = mostRecentFunctionDef;
        }
    }

}
