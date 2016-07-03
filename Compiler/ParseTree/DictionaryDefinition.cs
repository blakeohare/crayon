using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
	internal class DictionaryDefinition : Expression
	{
		public override bool CanAssignTo { get { return false; } }

		public Expression[] Keys { get; private set; }
		public Expression[] Values { get; private set; }

		public DictionaryDefinition(Token braceToken, IList<Expression> keys, IList<Expression> values, Executable owner)
			: base(braceToken, owner)
		{
			this.Keys = keys.ToArray();
			this.Values = values.ToArray();
		}

		internal override Expression Resolve(Parser parser)
		{
			for (int i = 0; i < this.Keys.Length; ++i)
			{
				this.Keys[i] = this.Keys[i].Resolve(parser);
				this.Values[i] = this.Values[i].Resolve(parser);
				// TODO: verify no duplicate keys?
			}
			return this;
		}

		internal override void SetLocalIdPass(VariableIdAllocator varIds)
		{
			for (int i = 0, length = this.Keys.Length; i < length; ++i)
			{
				this.Keys[i].SetLocalIdPass(varIds);
				this.Values[i].SetLocalIdPass(varIds);
			}	
		}

		internal override Expression ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
		{
			this.BatchExpressionNameResolver(parser, lookup, imports, this.Keys);
			this.BatchExpressionNameResolver(parser, lookup, imports, this.Values);
			return this;
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars)
        {
            for (int i = 0; i < this.Keys.Length; ++i)
            {
                this.Keys[i].GetAllVariablesReferenced(vars);
                this.Values[i].GetAllVariablesReferenced(vars);
            }
        }
    }
}
