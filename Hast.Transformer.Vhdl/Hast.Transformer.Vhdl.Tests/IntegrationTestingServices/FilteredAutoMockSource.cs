using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Core;
using Hast.TestBase.Services;
using Moq;

namespace Hast.Transformer.Vhdl.Tests.IntegrationTestingServices
{
    public class FilteredAutoMockSource : IRegistrationSource
    {
        public AutoMockService AutoMockSource { get; } = new AutoMockService(MockBehavior.Loose);
        public bool IsAdapterForIndividualComponents => false;


        public IEnumerable<IComponentRegistration> RegistrationsFor(
            Service service,
            Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
        {
            if (registrationAccessor(service).Any()) return Enumerable.Empty<IComponentRegistration>();

            return ((IRegistrationSource)AutoMockSource).RegistrationsFor(service, registrationAccessor);
        }
    }
}
