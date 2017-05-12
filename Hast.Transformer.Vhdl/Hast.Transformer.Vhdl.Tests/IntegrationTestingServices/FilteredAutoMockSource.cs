using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac.Core;
using Moq;
using Orchard.Tests.Utility;

namespace Hast.Transformer.Vhdl.Tests.IntegrationTestingServices
{
    public class FilteredAutoMockSource : IRegistrationSource
    {
        public ContainerExtensions.AutoMockSource AutoMockSource { get; } = new ContainerExtensions.AutoMockSource(MockBehavior.Loose);
        public bool IsAdapterForIndividualComponents { get { return AutoMockSource.IsAdapterForIndividualComponents; } }


        public IEnumerable<IComponentRegistration> RegistrationsFor(
            Service service,
            Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
        {
            if (registrationAccessor(service).Any()) return Enumerable.Empty<IComponentRegistration>();

            return ((IRegistrationSource)AutoMockSource).RegistrationsFor(service, registrationAccessor);
        }
    }
}
