using System.Collections.Generic;

namespace Crayon.Translator
{
	internal abstract class AbstractPlatformImplementation
	{
		public bool IsMin { get; private set; }
		public bool IsFull { get { return !this.IsMin; } }

		public AbstractPlatformImplementation(bool minified)
		{
			this.IsMin = minified;
		}


		public abstract string SerializeBoilerPlates(Parser parser);
	}
}
