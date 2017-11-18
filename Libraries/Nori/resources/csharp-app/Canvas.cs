using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Libraries.Nori
{
	public partial class Element
	{
		public class Canvas : System.Windows.Forms.Panel
		{
			public void SetPosition(int x, int y, int width, int height)
			{
				this.Location = new System.Drawing.Point(x, y);
				this.Size = new System.Drawing.Size(width, height);
			}

			public System.Windows.Forms.Control[] Children
			{
				get { return NoriHelper.GetControls(this.Controls); }
			}

			public override string ToString()
			{
				return "Canvas: <" + this.Location.X + ", " + this.Location.Y + ", " + this.Size.Width + ", " + this.Size.Height + ">";
			}
		}
    }
}
