using System.Threading.Tasks;
using Hast.Transformer.Models;
using Hast.Transformer.Vhdl.Models;
using Orchard.FileSystems.AppData;

namespace Hast.Transformer.Vhdl.Services
{
    public class VhdlHardwareDescriptionCachingService : IVhdlHardwareDescriptionCachingService
    {
        private readonly IAppDataFolder _appDataFolder;


        public VhdlHardwareDescriptionCachingService(IAppDataFolder appDataFolder)
        {
            _appDataFolder = appDataFolder;
        }


        public async Task<VhdlHardwareDescription> GetHardwareDescription(ITransformationContext transformationContext)
        {
            var filePath = GetCacheFilePath(transformationContext);

            if (!_appDataFolder.FileExists(filePath)) return null;

            using (var fileStream = _appDataFolder.OpenFile(filePath))
            {
                return await VhdlHardwareDescription.Load(fileStream);
            }
        }

        public async Task SetHardwareDescription(ITransformationContext transformationContext, VhdlHardwareDescription hardwareDescription)
        {
            using (var fileStream = _appDataFolder.CreateFile(GetCacheFilePath(transformationContext)))
            {
                await hardwareDescription.Save(fileStream);
            }
        }


        private string GetCacheFilePath(ITransformationContext transformationContext)
        {
            return _appDataFolder.Combine(
                "Hastlayer", 
                "VhdlHardwareDescriptionCacheFiles", 
                // These could be SHA256 hashes too, as all hashes were the result is persisted. However that way the
                // path would be too long... And here it doesn't really matter because caches are local to the machine
                // any way (and on the same machine GetHashCode() will give the same result for the same input).
                transformationContext.SyntaxTree.ToString().GetHashCode().ToString() + "-" + transformationContext.Id.GetHashCode());
        }
    }
}
