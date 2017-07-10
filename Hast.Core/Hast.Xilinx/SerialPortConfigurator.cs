using System.IO.Ports;
using Hast.Common.Extensibility.Pipeline;
using Hast.Communication.Extensibility.Pipeline;
using Hast.Communication.Models;

namespace Hast.Xilinx
{
    public class SerialPortConfigurator : PipelineStepBase, ISerialPortConfigurator
    {
        public void ConfigureSerialPort(SerialPort serialPort, IHardwareExecutionContext hardwareExecutionContext)
        {
            if (hardwareExecutionContext.HardwareRepresentation.DeviceManifest.Name != Nexys4DdrDriver.DeviceName) return;

            serialPort.BaudRate = 230400;
        }
    }
}
