using System.Threading.Tasks;
using Hast.Transformer.Models;
using Hast.Transformer.Vhdl.Models;
using Orchard;

namespace Hast.Transformer.Vhdl.Services
{
    public interface IVhdlHardwareDescriptionCachingService : IDependency
    {
        Task<VhdlHardwareDescription> GetHardwareDescription(ITransformationContext transformationContext);
        Task SetHardwareDescription(ITransformationContext transformationContext, VhdlHardwareDescription hardwareDescription);
    }
}
