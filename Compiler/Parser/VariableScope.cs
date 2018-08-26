using System.Collections.Generic;

namespace Parser
{
    internal enum VariableIdAllocPhase
    {
        REGISTER = 0x1,

        ALLOC = 0x2,

        REGISTER_AND_ALLOC = 0x3,
    }

    public class VariableId
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }

    internal class VariableScope
    {
        private VariableScope underlyingInstance = null;
        private readonly Dictionary<string, VariableId> idsByVar = new Dictionary<string, VariableId>();
        private List<string> orderedVars = new List<string>();

        public int Size { get { return this.idsByVar.Count + (underlyingInstance == null ? 0 : underlyingInstance.Size); } }

        public void RegisterVariable(string value)
        {
            if (underlyingInstance != null && underlyingInstance.idsByVar.ContainsKey(value))
            {
                return;
            }

            if (!this.idsByVar.ContainsKey(value))
            {
                idsByVar[value] = new VariableId() { ID = idsByVar.Count, Name = value };
                orderedVars.Add(value);
            }
        }

        public VariableId GetVarId(Token variableToken)
        {
            VariableId id;
            if (this.idsByVar.TryGetValue(variableToken.Value, out id))
            {
                return id;
            }

            if (this.underlyingInstance != null)
            {
                return this.underlyingInstance.GetVarId(variableToken);
            }

            return null;
        }

        public VariableScope Clone()
        {
            VariableScope output = new VariableScope();
            output.underlyingInstance = this;
            return output;
        }

        public void MergeClonesBack(params VariableScope[] branches)
        {
            foreach (VariableScope branch in branches)
            {
                if (this != branch.underlyingInstance)
                {
                    throw new System.InvalidOperationException("Cannot merge two branches that aren't from the same root.");
                }
            }

            foreach (VariableScope branch in branches)
            {
                foreach (string varId in branch.orderedVars)
                {
                    this.RegisterVariable(varId);
                }
            }
        }
    }
}
