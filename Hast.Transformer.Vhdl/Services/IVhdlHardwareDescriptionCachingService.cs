using Hast.Common.Models;
using Hast.Transformer.Models;
using Hast.Common.Interfaces;
using System.Threading.Tasks;

namespace Hast.Transformer.Vhdl.Services
{
    public interface IVhdlHardwareDescriptionCachingService : IDependency
    {
        Task<VhdlHardwareDescription> GetHardwareDescriptionAsync(string cacheKey);
        Task SetHardwareDescriptionAsync(string cacheKey, VhdlHardwareDescription hardwareDescription);
    }
}
