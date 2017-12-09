using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Hast.Layer;
using Hast.Transformer.Abstractions;
using Hast.Transformer.Vhdl.Abstractions.Configuration;
using Hast.Transformer.Vhdl.Models;

namespace Hast.Transformer.Vhdl.Tests.VerificationTests
{
    public abstract class VerificationTestFixtureBase : VhdlTransformingTestFixtureBase
    {
        protected override Task<VhdlHardwareDescription> TransformAssembliesToVhdl(
           ITransformer transformer,
           IEnumerable<Assembly> assemblies,
           Action<HardwareGenerationConfiguration> configurationModifier = null)
        {
            return base.TransformAssembliesToVhdl(
                transformer, 
                assemblies, 
                configuration =>
                {
                    configuration.VhdlTransformerConfiguration().VhdlGenerationConfiguration = VhdlGenerationConfiguration.Debug;
                    configurationModifier?.Invoke(configuration);
                });
        }
    }
}
