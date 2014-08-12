using System.Collections.Generic;

namespace Crayon.Translator
{
	internal abstract class AbstractPlatformImplementation
	{
		public abstract void SerializeBoilerPlates(Parser parser, List<string> output);
	}
}
