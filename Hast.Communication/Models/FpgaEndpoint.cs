using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Communication.Models
{
    public class FpgaEndpoint : IFpgaEndpoint
    {
        public bool IsAvailable { get; set; }

        public IPEndPoint Endpoint { get; set; }

        public DateTime LastCheckedUtc { get; set; }
    }
}
