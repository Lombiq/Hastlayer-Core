using Hast.Communication.Exceptions;
using Hast.Transformer.SimpleMemory;
using Orchard.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Threading.Tasks;
using Hast.Communication.Models;
using Hast.Common.Models;
using Hast.Communication.Constants;
using Hast.Synthesis;

namespace Hast.Communication.Services
{
    public abstract class CommunicationServiceBase : ICommunicationService
    {
        private Stopwatch _stopwatch;


        public ILogger Logger { get; set; }

        abstract public string ChannelName { get; }


        public CommunicationServiceBase()
        {
            Logger = NullLogger.Instance;
        }


        abstract public Task<IHardwareExecutionInformation> Execute(SimpleMemory simpleMemory, int memberId);


        protected HardwareExecutionInformation BeginExecutionTimer()
        {
            // Stopwatch for measuring the total exection time.
            _stopwatch = Stopwatch.StartNew();

            return new HardwareExecutionInformation();
        }

        protected void EndExecutionTimer(HardwareExecutionInformation executionInformation)
        {
            _stopwatch.Stop();

            executionInformation.FullExecutionTimeMilliseconds = _stopwatch.ElapsedMilliseconds;

            Logger.Information("Full execution time: {0}ms", _stopwatch.ElapsedMilliseconds);
        }

        protected void SetHardwareExecutionTime(HardwareExecutionInformation executionInformation, uint clockFrequencyHz, ulong executionTimeClockCycles)
        {
            executionInformation.HardwareExecutionTimeMilliseconds = 1M / clockFrequencyHz * 1000 * executionTimeClockCycles;

            Logger.Information("Hardware execution took " + executionInformation.HardwareExecutionTimeMilliseconds + "ms.");
        }
    }
}
