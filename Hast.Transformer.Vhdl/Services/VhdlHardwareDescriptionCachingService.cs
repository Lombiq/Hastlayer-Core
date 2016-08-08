using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                transformationContext.SyntaxTree.ToString().GetHashCode().ToString() + "-" + transformationContext.Id.GetHashCode());
        }
    }
}
