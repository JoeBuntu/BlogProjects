using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace SharedTools
{
    public class OperationTimer : IDisposable
    {
        private Stopwatch _StopWatch;

        public OperationTimer()
        {
            _StopWatch = new Stopwatch();
            _StopWatch.Start();
        }

        public void Dispose()
        {
            _StopWatch.Stop();
            System.Diagnostics.Trace.WriteLine("OperationTimer- Time Elapsed: " + _StopWatch.ElapsedMilliseconds.ToString());
        }
    }
}
