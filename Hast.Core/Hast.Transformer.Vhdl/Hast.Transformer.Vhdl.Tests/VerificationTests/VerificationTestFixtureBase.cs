using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Hast.Common.Configuration;
using Hast.Transformer.Vhdl.Abstractions.Configuration;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation;

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
                    configuration.VhdlTransformerConfiguration().VhdlGenerationMode = VhdlGenerationMode.Debug;
                    configurationModifier?.Invoke(configuration);
                });
        }
    }
}
