namespace Interpreter.Libraries.Nori
{
	public partial class Element
	{
		public class Rectangle : System.Windows.Forms.Panel
		{
			public void SetPosition(int x, int y, int width, int height)
			{
				this.Location = new System.Drawing.Point(x, y);
				this.Size = new System.Drawing.Size(width, height);
			}

			public  void SetColor(int r, int g, int b, int a)
			{
				this.BackColor = System.Drawing.Color.FromArgb(a, r, g, b);
			}

			public override string ToString()
			{
				return "Rectangle: <" + this.Location.X + ", " + this.Location.Y + ", " + this.Size.Width + ", " + this.Size.Height + "> RGB<" + this.BackColor.R + ", " + this.BackColor.G + ", " + this.BackColor.B + ", " + this.BackColor.A + ">";
			}
		}
    }
}
