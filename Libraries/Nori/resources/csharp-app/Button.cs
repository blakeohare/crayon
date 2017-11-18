using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Libraries.Nori
{
	public partial class Element
	{
		public class Button : System.Windows.Forms.Button
		{
			public void SetPosition(int x, int y, int width, int height)
			{
				this.Location = new System.Drawing.Point(x, y);
				this.Size = new System.Drawing.Size(width, height);
			}

			public override string ToString()
			{
				return "Button: <'" + this.Text + "', " + this.Location.X + ", " + this.Location.Y + ", " + this.Size.Width + ", " + this.Size.Height + ">";
			}
		}
    }
}
