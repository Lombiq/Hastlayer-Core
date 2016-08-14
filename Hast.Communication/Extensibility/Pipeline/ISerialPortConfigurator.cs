using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Common.Extensibility.Pipeline;

namespace Hast.Communication.Extensibility.Pipeline
{
    /// <summary>
    /// Extension point for modifying the default configuration used for serial communication.
    /// </summary>
    public interface ISerialPortConfigurator : IPipelineStep
    {
        void ConfigureSerialPort(SerialPort serialPort);
    }
}
