using Hast.Common.Interfaces;
using Hast.Transformer.Vhdl.Models;

namespace Hast.Transformer.Vhdl.Events
{
    public interface IVhdlTransformationEventHandler : IEventHandler
    {
        void TransformedVhdlManifestBuilt(ITransformedVhdlManifest transformedVhdlManifest);
    }
}
