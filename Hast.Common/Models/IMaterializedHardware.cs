using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Common.Models
{
    /// <summary>
    /// Represents a handle to the materialized hardware.
    /// </summary>
    public interface IMaterializedHardware
    {
        /// <summary>
        /// The hardware representation of the transformed assemblies.
        /// </summary>
        IHardwareRepresentation HardwareRepresentation { get; }
    }
}
