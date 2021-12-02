namespace Interpreter.Structs
{
    public class PlatformRelayObject
    {
        public int type;
        public int iarg1;
        public int iarg2;
        public int iarg3;
        public double farg1;
        public string sarg1;

        public PlatformRelayObject(int type, int iarg1, int iarg2, int iarg3, double farg1, string sarg1)
        {
            this.type = type;
            this.iarg1 = iarg1;
            this.iarg2 = iarg2;
            this.iarg3 = iarg3;
            this.farg1 = farg1;
            this.sarg1 = sarg1;
        }
    }

}
