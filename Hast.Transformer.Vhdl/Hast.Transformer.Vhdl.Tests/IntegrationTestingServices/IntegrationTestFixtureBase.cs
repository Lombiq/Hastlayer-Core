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
        protected IHastlayerConfiguration _hostConfiguration = HastlayerConfiguration.Default;

        private Lazy<IHastlayer> _host;
        protected IHastlayer Host => _host.Value;


        public IntegrationTestFixtureBase()
        {
            _hostConfiguration.Extensions = new List<Assembly>();
            _host = new Lazy<IHastlayer>(() => Hastlayer.Create(_hostConfiguration).Result);
        }

        public void Dispose()
        {
            Host.Dispose();
        }
    }
}
