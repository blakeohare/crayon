using System.Collections.Generic;

namespace Crayon
{
    internal enum VariableIdAllocPhase
    {
        REGISTER = 0x1,

        ALLOC = 0x2,

        REGISTER_AND_ALLOC = 0x3,
    }

    internal class VariableIdAllocator
    {
        private VariableIdAllocator underlyingInstance = null;
        private readonly Dictionary<string, int> idsByVar = new Dictionary<string, int>();
        private Dictionary<int, string> varsById = new Dictionary<int, string>();
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
                idsByVar[value] = varsById.Count;
                varsById.Add(this.Size, value);
                orderedVars.Add(value);
            }
        }

        public int GetVarId(Token variableToken)
        {
            int id;
            if (this.idsByVar.TryGetValue(variableToken.Value, out id))
            {
                return id;
            }

            if (this.underlyingInstance != null)
            {
                return this.underlyingInstance.GetVarId(variableToken);
            }

            return -1;
        }

        public VariableIdAllocator Clone()
        {
            VariableIdAllocator output = new VariableIdAllocator();
            output.underlyingInstance = this;
            return output;
        }

        public void MergeClonesBack(params VariableIdAllocator[] branches)
        {
            foreach (VariableIdAllocator branch in branches)
            {
                if (this != branch.underlyingInstance)
                {
                    throw new System.InvalidOperationException("Cannot merge two branches that aren't from the same root.");
                }
            }

            foreach (VariableIdAllocator branch in branches)
            {
                foreach (string varId in branch.orderedVars)
                {
                    this.RegisterVariable(varId);
                }
            }
        }
    }
}
