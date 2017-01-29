using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon.Pastel
{
    class SwitchStatementTracker
    {
        private Dictionary<string, Dictionary<string, int>> stringSwitchLookups = new Dictionary<string, Dictionary<string, int>>();
        private Dictionary<string, Dictionary<int, int>> intListLookups = new Dictionary<string, Dictionary<int, int>>();
        private Dictionary<string, int> explicitMaxes = new Dictionary<string, int>();
        private Dictionary<string, int> defaultCaseIds = new Dictionary<string, int>();

        // These are the lookup tables for switch statements. The ID of the switch statement is its index in this list.
        private List<Dictionary<string, int>> byteCodeSwitchStringToOffsets = new List<Dictionary<string, int>>();
        private List<Dictionary<int, int>> byteCodeSwitchIntegerToOffsets = new List<Dictionary<int, int>>();

        public List<Dictionary<int, int>> GetIntegerSwitchStatements()
        {
            return this.byteCodeSwitchIntegerToOffsets;
        }

        public List<Dictionary<string, int>> GetStringSwitchStatements()
        {
            return this.byteCodeSwitchStringToOffsets;
        }

        public int RegisterByteCodeSwitch(Token switchToken, Dictionary<int, int> chunkIdsToOffsets, Dictionary<int, int> integersToChunkIds, Dictionary<string, int> stringsToChunkIds, bool isIntegerSwitch)
        {
            int switchId;
            if (isIntegerSwitch)
            {
                switchId = byteCodeSwitchIntegerToOffsets.Count;
                Dictionary<int, int> integersToOffsets = new Dictionary<int, int>();
                foreach (int key in integersToChunkIds.Keys)
                {
                    int chunkId = integersToChunkIds[key];
                    integersToOffsets[key] = chunkIdsToOffsets[chunkId];
                }
                byteCodeSwitchIntegerToOffsets.Add(integersToOffsets);
            }
            else
            {
                switchId = byteCodeSwitchStringToOffsets.Count;
                Dictionary<string, int> stringsToOffsets = new Dictionary<string, int>();
                foreach (string key in stringsToChunkIds.Keys)
                {
                    int chunkId = stringsToChunkIds[key];
                    stringsToOffsets[key] = chunkIdsToOffsets[chunkId];
                }
                byteCodeSwitchStringToOffsets.Add(stringsToOffsets);
            }
            return switchId;
        }

        public void RegisterSwitchIntegerListLookup(string name, Dictionary<int, int> lookup, int explicitMax, int defaultCaseId)
        {
            this.explicitMaxes[name] = explicitMax;
            this.defaultCaseIds[name] = defaultCaseId;
            this.intListLookups[name] = lookup;
        }

        public void RegisterSwitchStringDictLookup(string name, Dictionary<string, int> lookup)
        {
            this.stringSwitchLookups[name] = lookup;
        }

        public string GetSwitchLookupCode()
        {
            List<string> output = new List<string>();
            foreach (string key in this.stringSwitchLookups.Keys)
            {
                string lookupName = key;
                Dictionary<string, int> valuesToInts = this.stringSwitchLookups[key];
                output.Add(lookupName);
                output.Add(" = { ");
                bool first = true;
                foreach (string sKey in valuesToInts.Keys)
                {
                    if (!first)
                    {
                        first = false;
                        output.Add(", ");
                    }
                    output.Add(Util.ConvertStringValueToCode(sKey));
                    output.Add(": ");
                    output.Add("" + valuesToInts[sKey]);
                }
                output.Add(" };\r\n");
            }

            foreach (string lookupName in this.intListLookups.Keys)
            {
                List<int> actualList = new List<int>();
                Dictionary<int, int> lookup = this.intListLookups[lookupName];
                int explicitMax = this.explicitMaxes[lookupName];
                int defaultCaseId = this.defaultCaseIds[lookupName];
                while (actualList.Count <= explicitMax)
                {
                    actualList.Add(defaultCaseId);
                }

                foreach (int iKey in lookup.Keys)
                {
                    while (actualList.Count <= iKey)
                    {
                        actualList.Add(defaultCaseId);
                    }
                    actualList[iKey] = lookup[iKey];
                }

                output.Add(lookupName);
                output.Add(" = [");
                for (int i = 0; i < actualList.Count; ++i)
                {
                    if (i > 0) output.Add(", ");
                    output.Add(actualList[i] + "");
                }
                output.Add("];\r\n");
            }

            return string.Join("", output);
        }
    }
}
