using System.Threading.Tasks;
using Hast.Common.Interfaces;
using Hast.Transformer.Models;
using Hast.Transformer.Vhdl.Models;

namespace Hast.Transformer.Vhdl.Services
{
    public interface ITransformedVhdlManifestBuilder : IDependency
    {
        Task<ITransformedVhdlManifest> BuildManifestAsync(ITransformationContext transformationContext);
    }
}
