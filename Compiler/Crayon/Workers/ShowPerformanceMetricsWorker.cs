using Common;
using System;

namespace Crayon
{
    internal class ShowPerformanceMetricsWorker
    {
        public void DoWork()
        {
#if DEBUG
            string summary = PerformanceTimer.GetSummary();
            Console.WriteLine(summary);
#endif
        }
    }
}
