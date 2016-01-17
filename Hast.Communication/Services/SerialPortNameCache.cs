using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orchard;

namespace Hast.Communication.Services
{
    /// <summary>
    /// Caches the name of the serial port where a compatible FPGA is connected.
    /// </summary>
    /// <remarks>
    /// We make the bold guess that it's valid to cache the port name for the lifetime of the shell.
    /// </remarks>
    public interface ISerialPortNameCache : ISingletonDependency
    {
        string PortName { get; set; }
    }


    public class SerialPortNameCache : ISerialPortNameCache
    {
        // Since this is a singleton we need to care about concurrent access.
        private readonly object _lock = new object();

        private string _portName;
        public string PortName
        {
            get
            {
                lock (_lock)
                {
                    return _portName; 
                }
            }

            set
            {
                lock (_lock)
                {
                    _portName = value; 
                }
            }
        }
    }
}
