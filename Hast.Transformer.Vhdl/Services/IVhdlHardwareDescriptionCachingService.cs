using Hast.Common.Models;
using Hast.Transformer.Models;
using Orchard;
using System.Threading.Tasks;

namespace Hast.Transformer.Vhdl.Services
{
    public interface IVhdlHardwareDescriptionCachingService : IDependency
    {
        Task<VhdlHardwareDescription> GetHardwareDescription(string cacheKey);
        Task SetHardwareDescription(string cacheKey, VhdlHardwareDescription hardwareDescription);
        string GetCacheKey(ITransformationContext transformationContext);
    }
}
