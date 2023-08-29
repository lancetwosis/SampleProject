using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRedminePower.Logging
{
    public class StopWatchLogger : IDisposable
    {
        private Stopwatch sw;
        private string proccessName;

        public StopWatchLogger(string proccessName)
        {
            this.sw = new Stopwatch();
            sw.Start();
            this.proccessName = proccessName;
            Logger.Info($"{proccessName} was started.");
        }

        public void Dispose()
        {
            sw.Stop();
            var ms = (double)sw.ElapsedMilliseconds / (double)1000;
            Logger.Info($"{proccessName} was finished. Time = {ms} [sec]");
        }
    }
}
