using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon.ParseTree
{
	internal class ForEachLoop : Executable
	{
		public Token IterationVariable { get; private set; }
		public int IterationVariableId { get; private set; }
		public Expression IterationExpression { get; private set; }
		public Executable[] Code { get; private set; }
 
		public ForEachLoop(Token forToken, Token iterationVariable, Expression iterationExpression, IList<Executable> code, Executable owner)
			: base(forToken, owner)
		{
			this.IterationVariable = iterationVariable;
			this.IterationExpression = iterationExpression;
			this.Code = code.ToArray();
		}

		internal override IList<Executable> Resolve(Parser parser)
		{
			this.IterationExpression = this.IterationExpression.Resolve(parser);
			this.Code = Resolve(parser, this.Code).ToArray();
			return Listify(this);
		}

		internal override void GenerateGlobalNameIdManifest(VariableIdAllocator varIds)
		{
			string iterator = this.IterationVariable.Value;
			varIds.RegisterVariable(iterator);

			foreach (Executable ex in this.Code)
			{
				ex.GenerateGlobalNameIdManifest(varIds);
			}
		}

		internal override void CalculateLocalIdPass(VariableIdAllocator varIds)
		{
			varIds.RegisterVariable(this.IterationVariable.Value);
			for (int i = 0; i < this.Code.Length; ++i)
			{
				this.Code[i].CalculateLocalIdPass(varIds);
			}
		}

		internal override void SetLocalIdPass(VariableIdAllocator varIds)
		{
			this.IterationExpression.SetLocalIdPass(varIds);
			this.IterationVariableId = varIds.GetVarId(this.IterationVariable, false);
			for (int i = 0; i < this.Code.Length; ++i)
			{
				this.Code[i].SetLocalIdPass(varIds);
			}
		}

		internal override Executable ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
		{
			this.IterationExpression = this.IterationExpression.ResolveNames(parser, lookup, imports);
			this.BatchExecutableNameResolver(parser, lookup, imports, this.Code);
			return this;
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars)
        {
            this.IterationExpression.GetAllVariablesReferenced(vars);
            for (int i = 0; i < this.Code.Length; ++i)
            {
                this.Code[i].GetAllVariablesReferenced(vars);
            }
        }
    }
}
