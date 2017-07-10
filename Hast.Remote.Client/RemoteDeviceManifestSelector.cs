using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Layer;
using Hast.Synthesis.Abstractions;

namespace Hast.Remote.Client
{
    public class RemoteDeviceManifestSelector : IDeviceManifestSelector
    {
        public Task<IEnumerable<IDeviceManifest>> GetSupporteDevices()
        {
            throw new NotImplementedException();
        }
    }
}
