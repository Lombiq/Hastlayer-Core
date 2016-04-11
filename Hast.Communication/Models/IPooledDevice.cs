using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Communication.Models
{
    /// <summary>
    /// Represents a compatible device for hardware exeuction that is in the managed device pool.
    /// </summary>
    public interface IPooledDevice : IDevice
    {
        /// <summary>
        /// Gets whether the pooled device is busy with a hardware execution.
        /// </summary>
        bool IsBusy { get; }
    }
}
