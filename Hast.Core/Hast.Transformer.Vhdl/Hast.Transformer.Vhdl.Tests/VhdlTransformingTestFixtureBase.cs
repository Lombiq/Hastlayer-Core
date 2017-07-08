using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Hast.Common.Configuration;
using Hast.Common.Models;
using Hast.Synthesis;
using Hast.Synthesis.Models;
using Hast.Synthesis.Services;
using Hast.Transformer.Abstractions;
using Hast.Transformer.Models;
using Hast.Transformer.Services;
using Hast.Transformer.Vhdl.Models;
using Hast.Transformer.Vhdl.Tests.IntegrationTestingServices;
using ICSharpCode.NRefactory.CSharp;
using System.Linq;
using Autofac.Core;

namespace Hast.Transformer.Vhdl.Tests
{
    public abstract class VhdlTransformingTestFixtureBase : IntegrationTestFixtureBase
    {
        protected virtual bool UseStubMemberSuitabilityChecker { get; set; } = true;


        protected VhdlTransformingTestFixtureBase()
        {
            _requiredExtension.AddRange(new[] { typeof(DefaultTransformer).Assembly, typeof(MemberIdTable).Assembly });

            _shellRegistrationBuilder = builder =>
            {
                if (UseStubMemberSuitabilityChecker)
                {
                    // We need to override MemberSuitabilityChecker in Hast.Transformer. Since that registration happens
                    // after this one we need to use this hackish way over circumventing that.
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

                builder.RegisterInstance(new StubDeviceDriverSelector()).As<IDeviceDriverSelector>();
            };
        }


        protected virtual async Task<VhdlHardwareDescription> TransformAssembliesToVhdl(
            ITransformer transformer,
            IEnumerable<Assembly> assemblies,
            Action<HardwareGenerationConfiguration> configurationModifier = null)
        {
            var configuration = new HardwareGenerationConfiguration("Nexys4 DDR") { EnableCaching = false };
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

        private class StubDeviceDriverSelector : IDeviceDriverSelector
        {
            public IDeviceDriver GetDriver(string deviceName) => new StubDeviceDriver();

            public IEnumerable<IDeviceManifest> GetSupporteDevices()
            {
                throw new NotImplementedException();
            }


            private class StubDeviceDriver : IDeviceDriver
            {
                public IDeviceManifest DeviceManifest
                {
                    get
                    {
                        return new DeviceManifest
                        {
                            Name = "Nexys4 DDR",
                            ClockFrequencyHz = 100000000, // 100 Mhz
                            SupportedCommunicationChannelNames = new[] { "Serial", "Ethernet" },
                            AvailableMemoryBytes = 115343360 // 110MB
                        };
                    }
                }

                public decimal GetClockCyclesNeededForBinaryOperation(BinaryOperatorExpression expression, int operandSizeBits, bool isSigned)
                {
                    if (expression.Operator == BinaryOperatorType.Modulus) return 7M;

                    return 0.1M;
                }

                public decimal GetClockCyclesNeededForUnaryOperation(UnaryOperatorExpression expression, int operandSizeBits, bool isSigned) => 0.1M;
            }
        }
    }
}
