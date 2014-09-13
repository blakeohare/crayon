using System;
using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
	internal class SwitchStatementUnsafeBlotchy : Executable
	{
		public Expression Condition { get; private set; }
		public SwitchStatement OriginalSwitchStatement { get; private set; }

		public Dictionary<string, int> StringsToCaseIds { get; private set; }
		public Dictionary<int, int> IntegersToCaseIds { get; private set; }
		public int DefaultCaseId { get; private set; }
		public int ExplicitMax { get; private set; }
		public bool UseExplicitMax { get; private set; }

		private Dictionary<int, Executable[]> codeMapping;
		public Dictionary<string, int> StringUnsafeToSafeMapping;
		public Dictionary<int, int> IntegerUnsafeToSafeMapping;
		public bool UsesStrings { get; private set; }

		private int max;
		private static int counter = 0;
		private int id = ++counter;

		public SwitchStatementUnsafeBlotchy(SwitchStatement switchStatement, bool useExplicitMax, int explicitMax)
			: base(switchStatement.FirstToken)
		{
			this.OriginalSwitchStatement = switchStatement;
			this.Condition = switchStatement.Condition;

			this.UsesStrings = switchStatement.UsesStrings;
			this.UseExplicitMax = useExplicitMax;
			this.ExplicitMax = explicitMax;
			if (this.UsesStrings)
			{
				this.StringUnsafeToSafeMapping = new Dictionary<string, int>();
			}
			else
			{
				this.IntegerUnsafeToSafeMapping = new Dictionary<int, int>();
			}
			this.codeMapping = new Dictionary<int, Executable[]>();

			this.max = switchStatement.Chunks.Length - 1;

			for (int i = 0; i < switchStatement.Chunks.Length; ++i)
			{
				SwitchStatement.Chunk chunk = switchStatement.Chunks[i];
				this.codeMapping[i] = chunk.Code;
				foreach (Expression expression in chunk.Cases)
				{
					if (expression == null)
					{
						this.DefaultCaseId = i;
					}
					else
					{
						IntegerConstant ic = expression as IntegerConstant;
						StringConstant sc = expression as StringConstant;
						if (ic == null && sc == null) throw new Exception("This shouldn't happen.");
						if (ic != null)
						{
							int c = ic.Value;
							this.IntegerUnsafeToSafeMapping[c] = i;
						}
						else
						{
							string s = sc.Value;
							this.StringUnsafeToSafeMapping[s] = i;
						}
					}
				}
			}

			if (useExplicitMax)
			{
				if (this.UsesStrings) throw new Exception("Cannot use explicit max on string switch statements.");
			}
		}

		public SwitchStatementContinuousSafe.SearchTree GenerateSearchTree()
		{
			return SwitchStatementContinuousSafe.GenerateSearchTreeCommon(this.codeMapping, 0, this.max);
		}

		public string LookupTableName { get; private set; }

		public override IList<Executable> Resolve(Parser parser)
		{
			bool removeBreaks = parser.Mode == PlatformTarget.Python_PyGame;
			
			if (removeBreaks)
			{
				foreach (int key in this.codeMapping.Keys.ToArray()) // ToArray is used otherwise C# considers this modifying the enumerable collection
				{
					this.codeMapping[key] = Executable.RemoveBreaksForElifedSwitch(removeBreaks, this.codeMapping[key]);
				}

				this.LookupTableName = "switch_lookup_" + parser.GetNextInt();

				if (this.UsesStrings)
				{
					Dictionary<string, int> lookup = new Dictionary<string, int>();
					foreach (string key in this.StringUnsafeToSafeMapping.Keys)
					{
						lookup.Add(key, this.StringUnsafeToSafeMapping[key]);
					}

					parser.RegisterSwitchStringDictLookup(this.LookupTableName, lookup);
				}
				else
				{
					Dictionary<int, int> lookup = new Dictionary<int, int>();
					foreach (int key in this.IntegerUnsafeToSafeMapping.Keys)
					{
						lookup.Add(key, this.IntegerUnsafeToSafeMapping[key]);
					}

					parser.RegisterSwitchIntegerListLookup(this.LookupTableName, lookup, this.UseExplicitMax ? this.ExplicitMax : 0, this.DefaultCaseId);
				}
			}

			return Listify(this);
		}

		public override void GetAllVariableNames(Dictionary<string, bool> lookup)
		{
			this.OriginalSwitchStatement.GetAllVariableNames(lookup);
		}
	}
}
