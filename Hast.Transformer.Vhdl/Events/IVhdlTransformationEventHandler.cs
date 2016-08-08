using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Vhdl.Models;
using Orchard.Events;

namespace Hast.Transformer.Vhdl.Events
{
    public interface IVhdlTransformationEventHandler : IEventHandler
    {
        void TransformedVhdlManifestBuilt(ITransformedVhdlManifest transformedVhdlManifest);
    }
}
