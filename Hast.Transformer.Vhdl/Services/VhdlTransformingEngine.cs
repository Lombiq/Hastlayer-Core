using Hast.Common.Models;
using Hast.Layer;
using Hast.Transformer.Models;
using Hast.Transformer.Vhdl.Events;
using Hast.VhdlBuilder.Representation;
using System.Threading.Tasks;

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
            var cacheKey = _vhdlHardwareDescriptionCachingService.GetCacheKey(transformationContext);

            if (transformationContext.HardwareGenerationConfiguration.EnableCaching)
            {
                var cachedHardwareDescription = await _vhdlHardwareDescriptionCachingService
                    .GetHardwareDescription(cacheKey);
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

            var xdcSource = transformedVhdlManifest.XdcFile != null ?
                transformedVhdlManifest.XdcFile.ToVhdl(vhdlGenerationOptions) :
                null;
            var hardwareDescription = new VhdlHardwareDescription
            {
                HardwareEntryPointNamesToMemberIdMappings = transformedVhdlManifest.MemberIdTable.Mappings,
                VhdlSource = transformedVhdlManifest.Manifest.ToVhdl(vhdlGenerationOptions),
                Warnings = transformedVhdlManifest.Warnings,
                XdcSource = xdcSource
            };

            if (transformationContext.HardwareGenerationConfiguration.EnableCaching)
            {
                await _vhdlHardwareDescriptionCachingService.SetHardwareDescription(cacheKey, hardwareDescription);
            }

            return hardwareDescription;
        }
    }
}
