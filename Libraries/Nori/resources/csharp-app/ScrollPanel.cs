namespace Interpreter.Libraries.Nori
{
    public class ScrollPanel : System.Windows.Forms.Panel
    {
        public ScrollPanel() : base()
        {
            this.AutoScroll = true;
        }

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
            return "ScrollPanel: <" + this.Location.X + ", " + this.Location.Y + ", " + this.Size.Width + ", " + this.Size.Height + ">";
        }
    }
}
