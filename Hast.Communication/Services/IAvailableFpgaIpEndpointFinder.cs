﻿using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Communication.Services
{
    /// <summary>
    /// Service for finding available FPGAs attached to our network (eg. FPGAs on the same LAN like the PC).
    /// </summary>
    public interface IAvailableFpgaIpEndpointFinder : IDependency
    {
        /// <summary>
        /// Returns an FPGA endpoint that is currently available and ready to receive our messages.
        /// </summary>
        /// <returns>An available FPGA endpoint.</returns>
        Task<IPEndPoint> FindAnAvailableFpgaIpEndpoint();
    }
}
