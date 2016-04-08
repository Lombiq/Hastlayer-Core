using Hast.Communication.Models;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Communication.Services
{
    /// <summary>
    /// Service for finding FPGA endpoint attached to the same LAN like the PC.
    /// </summary>
    public interface IFpgaIpEndpointFinder : IDependency
    {
        /// <summary>
        /// Returns all FPGA endpoint that is currently attached to the network.
        /// </summary>
        /// <returns>FPGA endpoints attached to the network.</returns>
        Task<IEnumerable<IFpgaEndpoint>> FindFpgaEndpoints();
    }
}
