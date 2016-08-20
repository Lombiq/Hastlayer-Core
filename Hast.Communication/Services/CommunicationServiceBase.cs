using System.Diagnostics;
using System.Threading.Tasks;
using Hast.Common.Models;
using Hast.Communication.Models;
using Hast.Synthesis;
using Hast.Transformer.SimpleMemory;
using Orchard.Logging;

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

        protected void SetHardwareExecutionTime(CommunicationStateContext context, IDeviceDriver deviceDriver, ulong executionTimeClockCycles)
        {
            context.HardwareExecutionInformation.HardwareExecutionTimeMilliseconds = 
                1M / deviceDriver.DeviceManifest.ClockFrequencyHz * 1000 * executionTimeClockCycles;

            Logger.Information("Hardware execution took " + context.HardwareExecutionInformation.HardwareExecutionTimeMilliseconds + "ms.");
        }


        protected class CommunicationStateContext
        {
            public Stopwatch Stopwatch { get; set; }

            public HardwareExecutionInformation HardwareExecutionInformation { get; set; }
        }
    }
}
