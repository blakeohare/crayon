using System.Collections.Generic;

namespace Crayon.Translator.Python.PyGame
{
	internal class PyGameImplementation : AbstractPlatformImplementation
	{
		public override void SerializeBoilerPlates(Parser parser, List<string> output)
		{
			string pygameCode = Util.ReadFileInternally("Translator/Python/PyGame/PyGame.py");

			pygameCode = Constants.DoReplacements(pygameCode);

			output.Add(pygameCode);
			output.Add("\r\n");
		}
	}
}
