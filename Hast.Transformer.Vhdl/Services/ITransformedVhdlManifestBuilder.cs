using System.Threading.Tasks;
using Hast.Transformer.Models;
using Hast.Transformer.Vhdl.Models;
using Hast.Common.Interfaces;

namespace Hast.Transformer.Vhdl.Services
{
    public interface ITransformedVhdlManifestBuilder : IDependency
    {
        Task<ITransformedVhdlManifest> BuildManifestAsync(ITransformationContext transformationContext);
    }
}
