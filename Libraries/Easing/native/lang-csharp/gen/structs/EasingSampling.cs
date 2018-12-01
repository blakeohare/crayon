using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Libraries.Easing
{
    public class EasingSampling
    {
        public int sampleCount;
        public double[] samples;

        public EasingSampling(int sampleCount, double[] samples)
        {
            this.sampleCount = sampleCount;
            this.samples = samples;
        }
    }

}
