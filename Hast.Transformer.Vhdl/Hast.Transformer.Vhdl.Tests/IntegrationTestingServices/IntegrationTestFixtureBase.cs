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
        protected HastlayerConfiguration _hostConfiguration = new HastlayerConfiguration();

        private readonly Lazy<Hastlayer> _host;
        protected Hastlayer Host => _host.Value;


        public IntegrationTestFixtureBase()
        {
            _hostConfiguration.Extensions = new List<Assembly>();
            _host = new Lazy<Hastlayer>(() => (Hastlayer)Hastlayer.Create(_hostConfiguration));
        }

        public void Dispose()
        {
            Host.Dispose();
        }
    }
}
