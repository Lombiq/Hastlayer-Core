using Autofac;
using Autofac.Core;
using Hast.Layer;
using Hast.Synthesis.Services;
using Hast.Transformer.Abstractions;
using Hast.Transformer.Models;
using Hast.Transformer.Services;
using Hast.Transformer.Vhdl.Models;
using Hast.Transformer.Vhdl.Tests.IntegrationTestingServices;
using Hast.Xilinx;
using Hast.Xilinx.Abstractions;
using ICSharpCode.Decompiler.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Hast.Transformer.Vhdl.Tests
{
    public abstract class VhdlTransformingTestFixtureBase : IntegrationTestFixtureBase
    {
        protected virtual bool UseStubMemberSuitabilityChecker { get; set; } = true;
        protected virtual string DeviceName { get; set; } = Nexys4DdrManifestProvider.DeviceName;


        protected VhdlTransformingTestFixtureBase()
        {
            _requiredExtension.AddRange(new[]
            {
                typeof(DefaultTransformer).Assembly,
                typeof(MemberIdTable).Assembly,
                typeof(IDeviceDriverSelector).Assembly,
                typeof(Nexys4DdrDriver).Assembly
            });

            _shellRegistrationBuilder = builder =>
            {
                if (UseStubMemberSuitabilityChecker)
                {
                    // We need to override MemberSuitabilityChecker in Hast.Transformer. Since that registration happens
                    // after this one we need to use this hackish way of circumventing that.
                    builder.RegisterCallback(componentRegistry =>
                    {
                        var memberSuitabilityCheckerRegistration = componentRegistry
                            .Registrations
                            .Where(registration => registration
                                .Services
                                .Any(service => service is TypedService && ((TypedService)service).ServiceType == typeof(IMemberSuitabilityChecker)))
                            .SingleOrDefault();

                        if (memberSuitabilityCheckerRegistration == null) return;

                        memberSuitabilityCheckerRegistration.Activating += (sender, activatingEventArgs) =>
                        {
                            activatingEventArgs.Instance = new StubMemberSuitabilityChecker();
                        };
                    });
                }
            };
        }


        protected virtual async Task<VhdlHardwareDescription> TransformAssembliesToVhdl(
            ITransformer transformer,
            IEnumerable<Assembly> assemblies,
            Action<HardwareGenerationConfiguration> configurationModifier = null)
        {
            var configuration = new HardwareGenerationConfiguration(DeviceName) { EnableCaching = false };
            configurationModifier?.Invoke(configuration);
            return (VhdlHardwareDescription)await transformer.Transform(assemblies, configuration);
        }


        private class StubMemberSuitabilityChecker : IMemberSuitabilityChecker
        {
            public bool IsSuitableHardwareEntryPointMember(
                EntityDeclaration member,
                ITypeDeclarationLookupTable typeDeclarationLookupTable) =>
                (member.HasModifier(Modifiers.Public) && member.FindFirstParentTypeDeclaration().HasModifier(Modifiers.Public)) ||
                member.Modifiers == Modifiers.None;
        }
    }
}
