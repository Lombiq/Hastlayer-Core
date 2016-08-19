using Hast.Transformer.Vhdl.Models;
using Orchard.Events;

namespace Hast.Transformer.Vhdl.Events
{
    public interface IVhdlTransformationEventHandler : IEventHandler
    {
        void TransformedVhdlManifestBuilt(ITransformedVhdlManifest transformedVhdlManifest);
    }
}
