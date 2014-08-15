using System.Collections.Generic;

namespace Crayon.Translator.Python.PyGame
{
	internal class PyGameImplementation : AbstractPlatformImplementation
	{
		public override string SerializeBoilerPlates(Parser parser)
		{
			string pygameCode = Util.ReadFileInternally("Translator/Python/PyGame/PyGame.py");

			pygameCode = Constants.DoReplacements(pygameCode, parser.Mode);

			return pygameCode;
		}
	}
}
