using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Remote.Client
{
    public class HastlayerRemoteClientConfiguration
    {
        public Uri EndpointBaseUri { get; set; } = new Uri("http://hastlayer.com.127-0-0-1.org.uk/api/Hastlayer.Frontend");
        public string AppId { get; set; }
        public string AppSecret { get; set; }
    }
}
