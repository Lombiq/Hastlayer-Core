using System.Threading.Tasks;
using Hast.Common.Interfaces;
using Hast.Common.Models;

namespace Hast.Transformer.Vhdl.Services
{
    public interface IVhdlHardwareDescriptionCachingService : IDependency
    {
        Task<VhdlHardwareDescription> GetHardwareDescriptionAsync(string cacheKey);
        Task SetHardwareDescriptionAsync(string cacheKey, VhdlHardwareDescription hardwareDescription);
    }
}
