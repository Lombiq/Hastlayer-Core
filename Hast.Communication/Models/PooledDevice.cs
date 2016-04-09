using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Communication.Models
{
    public class PooledDevice : Device, IPooledDevice
    {
        public bool IsBusy { get; set; }


        public PooledDevice(IDevice baseDevice) : base(baseDevice)
        {
        }
    }
}
