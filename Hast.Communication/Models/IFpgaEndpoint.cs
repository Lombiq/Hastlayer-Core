using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Communication.Models
{
    public interface IFpgaEndpoint
    {
        IPEndPoint Endpoint { get; }

        bool IsAvailable { get; set; }

        DateTime LastCheckedUtc { get; set; }
    }
}
