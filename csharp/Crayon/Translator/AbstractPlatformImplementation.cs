using System.Collections.Generic;

namespace Crayon.Translator
{
	internal abstract class AbstractPlatformImplementation
	{
		public abstract string SerializeBoilerPlates(Parser parser);
	}
}
