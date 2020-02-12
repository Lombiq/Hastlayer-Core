using System.Threading.Tasks;
using Hast.Transformer.Models;
using Hast.Transformer.Vhdl.Models;
using Hast.Common.Interfaces;

namespace Hast.Transformer.Vhdl.Services
{
    public interface IVhdlHardwareDescriptionCachingService : IDependency
    {
        Task<VhdlHardwareDescription> GetHardwareDescription(string cacheKey);
        Task SetHardwareDescription(string cacheKey, VhdlHardwareDescription hardwareDescription);
        string GetCacheKey(ITransformationContext transformationContext);
    }
}
