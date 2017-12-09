using System.Threading.Tasks;
using Hast.Layer;
using Hast.Transformer.Models;
using Hast.Transformer.Vhdl.Abstractions.Configuration;
using Hast.Transformer.Vhdl.Events;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation;

namespace Hast.Transformer.Vhdl.Services
{
    public class VhdlTransformingEngine : ITransformingEngine
    {
        private readonly IVhdlHardwareDescriptionCachingService _vhdlHardwareDescriptionCachingService;
        private readonly ITransformedVhdlManifestBuilder _transformedVhdlManifestBuilder;
        private readonly IVhdlTransformationEventHandler _vhdlTransformationEventHandler;


        public VhdlTransformingEngine(
            IVhdlHardwareDescriptionCachingService vhdlHardwareDescriptionCachingService,
            ITransformedVhdlManifestBuilder transformedVhdlManifestBuilder,
            IVhdlTransformationEventHandler vhdlTransformationEventHandler)
        {
            _vhdlHardwareDescriptionCachingService = vhdlHardwareDescriptionCachingService;
            _transformedVhdlManifestBuilder = transformedVhdlManifestBuilder;
            _vhdlTransformationEventHandler = vhdlTransformationEventHandler;
        }


        public async Task<IHardwareDescription> Transform(ITransformationContext transformationContext)
        {
            if (transformationContext.HardwareGenerationConfiguration.EnableCaching)
            {
                var cachedHardwareDescription = await _vhdlHardwareDescriptionCachingService
                    .GetHardwareDescription(transformationContext);
                if (cachedHardwareDescription != null) return cachedHardwareDescription; 
            }

            var transformedVhdlManifest = await _transformedVhdlManifestBuilder.BuildManifest(transformationContext);
            _vhdlTransformationEventHandler.TransformedVhdlManifestBuilt(transformedVhdlManifest);

            var vhdlGenerationConfiguration = transformationContext
                .HardwareGenerationConfiguration
                .VhdlTransformerConfiguration()
                .VhdlGenerationConfiguration;

            var vhdlGenerationOptions = new VhdlGenerationOptions
            {
                OmitComments = !vhdlGenerationConfiguration.AddComments
            };

            if (vhdlGenerationConfiguration.ShortenNames)
            {
                vhdlGenerationOptions.NameShortener = VhdlGenerationOptions.SimpleNameShortener;
            }

            var vhdlSource = transformedVhdlManifest.Manifest.ToVhdl(vhdlGenerationOptions);
            var hardwareDescription = new VhdlHardwareDescription(vhdlSource, transformedVhdlManifest);

            if (transformationContext.HardwareGenerationConfiguration.EnableCaching)
            {
                await _vhdlHardwareDescriptionCachingService.SetHardwareDescription(transformationContext, hardwareDescription); 
            }

            return hardwareDescription;
        }
    }
}
