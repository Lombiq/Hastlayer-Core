using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hast.Common.Models;
using Hast.Transformer.Models;
using Hast.Transformer.Vhdl.Constants;
using Hast.Transformer.Vhdl.Models;
using Hast.Transformer.Vhdl.SubTransformers;
using Hast.VhdlBuilder;
using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;
using ICSharpCode.NRefactory.CSharp;
using Hast.Common.Extensions;
using Hast.Transformer.Vhdl.ArchitectureComponents;
using System;
using Orchard.Services;
using Hast.Transformer.Vhdl.InvocationProxyBuilders;
using Hast.Transformer.Vhdl.SimpleMemory;
using Hast.Transformer.Vhdl.Services;
using Hast.Transformer.Vhdl.Events;

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

            var vhdlSource = transformedVhdlManifest.Manifest.TopModule.ToVhdl(new VhdlGenerationOptions
            {
                FormatCode = true,
                NameShortener = VhdlGenerationOptions.SimpleNameShortener
            });
            var hardwareDescription = new VhdlHardwareDescription(vhdlSource, transformedVhdlManifest.MemberIdTable);

            if (transformationContext.HardwareGenerationConfiguration.EnableCaching)
            {
                await _vhdlHardwareDescriptionCachingService.SetHardwareDescription(transformationContext, hardwareDescription); 
            }

            return hardwareDescription;
        }
    }
}
