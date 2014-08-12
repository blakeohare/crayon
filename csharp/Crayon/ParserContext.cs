using System.Collections.Generic;

namespace Crayon
{
	internal class ParserContext
	{
		private Dictionary<string, ParseTree.StructDefinition> structureDefinitions = new Dictionary<string, ParseTree.StructDefinition>();
		private Dictionary<string, ParseTree.Expression> constantDefinitions = new Dictionary<string, ParseTree.Expression>();

	}
}
