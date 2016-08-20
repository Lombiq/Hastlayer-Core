﻿using System.IO.Ports;
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
