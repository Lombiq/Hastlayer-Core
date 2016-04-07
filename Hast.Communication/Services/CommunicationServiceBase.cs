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
        public ILogger Logger { get; set; }

        abstract public string ChannelName { get; }


        public CommunicationServiceBase()
        {
            Logger = NullLogger.Instance;
        }


        abstract public Task<IHardwareExecutionInformation> Execute(SimpleMemory simpleMemory, int memberId);


        protected CommunicationStateContext BeginExecution()
        {
            return new CommunicationStateContext
            {
                Stopwatch = Stopwatch.StartNew(),
                HardwareExecutionInformation = new HardwareExecutionInformation()
            };
        }

        protected void EndExecution(CommunicationStateContext context)
        {
            context.Stopwatch.Stop();

            context.HardwareExecutionInformation.FullExecutionTimeMilliseconds = context.Stopwatch.ElapsedMilliseconds;

            Logger.Information("Full execution time: {0}ms", context.Stopwatch.ElapsedMilliseconds);
        }

        protected void SetHardwareExecutionTime(CommunicationStateContext context, uint clockFrequencyHz, 
            ulong executionTimeClockCycles)
        {
            context.HardwareExecutionInformation.HardwareExecutionTimeMilliseconds = 
                1M / clockFrequencyHz * 1000 * executionTimeClockCycles;

            Logger.Information("Hardware execution took " + 
                context.HardwareExecutionInformation.HardwareExecutionTimeMilliseconds + "ms.");
        }


        protected class CommunicationStateContext
        {
            public Stopwatch Stopwatch { get; set; }

            public HardwareExecutionInformation HardwareExecutionInformation { get; set; }
        }
    }
}
