using Hast.Layer.Services;
using Hast.Transformer.Models;
using Hast.Transformer.Vhdl.Models;
using System.Threading.Tasks;

namespace Hast.Transformer.Vhdl.Services
{
    public class VhdlHardwareDescriptionCachingService : IVhdlHardwareDescriptionCachingService
    {
        private readonly IAppDataFolder _appDataFolder;


        public VhdlHardwareDescriptionCachingService(IAppDataFolder appDataFolder)
        {
            _appDataFolder = appDataFolder;
        }


        public async Task<VhdlHardwareDescription> GetHardwareDescription(string cacheKey)
        {
            var filePath = GetCacheFilePath(cacheKey);

            if (!_appDataFolder.FileExists(filePath)) return null;

            using (var fileStream = _appDataFolder.OpenFile(filePath))
                return await VhdlHardwareDescription.Deserialize(fileStream);
        }

        public async Task SetHardwareDescription(string cacheKey, VhdlHardwareDescription hardwareDescription)
        {
            using (var fileStream = _appDataFolder.CreateFile(GetCacheFilePath(cacheKey)))
                await hardwareDescription.Serialize(fileStream);
        }

        public string GetCacheKey(ITransformationContext transformationContext) =>
            // These could be SHA256 hashes too, as all hashes were the result is persisted. However that way the path 
            // would be too long... And here it doesn't really matter because caches are local to the machine any way 
            // (and on the same machine GetHashCode() will give the same result for the same input).
            transformationContext.SyntaxTree.ToString().GetHashCode().ToString() + "-" + transformationContext.Id.GetHashCode();


        private string GetCacheFilePath(string cacheKey)
            => _appDataFolder.Combine("Hastlayer", "VhdlHardwareDescriptionCacheFiles", cacheKey);
    }
}
