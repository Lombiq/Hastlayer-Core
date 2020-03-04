using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Hast.Layer;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Hast.Transformer.Vhdl.Tests.IntegrationTestingServices
{
    public abstract class IntegrationTestFixtureBase : IDisposable
    {
        protected List<Assembly> _requiredExtension = new List<Assembly>();
        //protected Action<ContainerBuilder> _shellRegistrationBuilder;
        protected IHastlayerConfiguration _hostConfiguration = HastlayerConfiguration.Default;
        protected IHastlayer _host;


        public IntegrationTestFixtureBase()
        {
            /*
            var settings = new AppHostSettings
            {
                ImportedExtensions = _requiredExtension,
                DefaultShellFeatureStates = new[]
                {
                    new DefaultShellFeatureState
                    {
                        EnabledFeatures = _requiredExtension.Select(extension => extension.ShortName())
                    }
                }
            };

            var autoMockSource = new FilteredAutoMockSource();
            autoMockSource.AutoMockSource.Ignore<IEventHandler>();
            autoMockSource.AutoMockSource.Ignore<IAsyncBackgroundTask>();

            var registrations = new AppHostRegistrations
            {
                ShellRegistrations = builder =>
                {
                    builder.RegisterSource(autoMockSource);

                    _shellRegistrationBuilder?.Invoke(builder);
                },
                HostRegistrations = builder =>
                {
                    builder.RegisterSource(autoMockSource);
                }
            };

            // Can't use async in NUnit 2.x in TestFixtureSetUp (available in 3.x).
            _host = OrchardAppHostFactory.StartTransientHost(settings, registrations, null).Result;
            
            */

            _hostConfiguration.Extensions = _requiredExtension;
            _host = Hastlayer.Create(_hostConfiguration).Result;
        }

        public void Dispose()
        {
            _host.Dispose();
        }
    }
}
