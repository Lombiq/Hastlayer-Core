using Hast.Common.Models;
using Hast.Layer;
using Hast.Transformer.Abstractions;
using Hast.Transformer.Vhdl.Abstractions.Configuration;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Hast.Transformer.Vhdl.Tests.VerificationTests
{
    public abstract class VerificationTestFixtureBase : VhdlTransformingTestFixtureBase
    {
        // Uncomment to make Shouldly open the KDiff version bundled with TortoiseHg.
        // static VerificationTestFixtureBase()
        // {
        //    var kDiffTortoiseHg = new DiffTool(
        //        "KDiffTortoiseHg",
        //        @"C:\Program Files\TortoiseHg\lib\kdiff3.exe",
        //        (received, approved, approvedExists) =>
        //            approvedExists ?
        //                $"\"{received}\" \"{approved}\" -o \"{approved}\"" :
        //                $"\"{received}\" -o \"{approved}\"");
        //    ShouldlyConfiguration.DiffTools.RegisterDiffTool(kDiffTortoiseHg);

        // ShouldlyConfiguration.DiffTools.SetDiffToolPriorities(kDiffTortoiseHg);
        // }

        protected override Task<VhdlHardwareDescription> TransformAssembliesToVhdl(
           ITransformer transformer,
           IList<Assembly> assemblies,
           Action<HardwareGenerationConfiguration> configurationModifier = null,
            string deviceName = null) =>
            base.TransformAssembliesToVhdl(
                transformer,
                assemblies,
                configuration =>
                {
                    configuration.VhdlTransformerConfiguration().VhdlGenerationConfiguration = VhdlGenerationConfiguration.Debug;
                    configurationModifier?.Invoke(configuration);
                },
                deviceName);
    }
}
