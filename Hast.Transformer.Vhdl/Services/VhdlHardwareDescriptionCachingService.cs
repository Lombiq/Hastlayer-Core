using Hast.Common.Models;
using Hast.Common.Services;
using Hast.Transformer.Models;
using System.Threading.Tasks;

namespace Hast.Transformer.Vhdl.Services
{
    public class VhdlHardwareDescriptionCachingService : IVhdlHardwareDescriptionCachingService
    {
        private readonly IAppDataFolder _appDataFolder;


        public VhdlHardwareDescriptionCachingService(IAppDataFolder appDataFolder) => _appDataFolder = appDataFolder;


        public async Task<VhdlHardwareDescription> GetHardwareDescription(string cacheKey)
        {
            var filePath = GetCacheFilePath(cacheKey);

            if (!_appDataFolder.FileExists(filePath)) return null;

            await using var fileStream = _appDataFolder.OpenFile(filePath);
            return await VhdlHardwareDescription.Deserialize(fileStream);
        }

        public Task SetHardwareDescription(string cacheKey, VhdlHardwareDescription hardwareDescription)
        {
            using var fileStream = _appDataFolder.CreateFile(GetCacheFilePath(cacheKey));
            return hardwareDescription.Serialize(fileStream);
        }


        private string GetCacheFilePath(string cacheKey)
            => _appDataFolder.Combine("Hastlayer", "VhdlHardwareDescriptionCacheFiles", cacheKey);
    }
}
