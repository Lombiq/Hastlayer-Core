using Hast.Layer;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Hast.Transformer.Vhdl.Tests.IntegrationTestingServices
{
    public abstract class IntegrationTestFixtureBase : IDisposable
    {
        private bool _disposed;
        protected HastlayerConfiguration _hostConfiguration = new();

        private readonly Lazy<Hastlayer> _host;
        protected Hastlayer Host => _host.Value;

        protected IntegrationTestFixtureBase()
        {
            _hostConfiguration.Extensions = new List<Assembly>();
            _host = new Lazy<Hastlayer>(() => (Hastlayer)Hastlayer.Create(_hostConfiguration));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing) Host.Dispose();

            _disposed = true;
        }

        ~IntegrationTestFixtureBase() => Dispose(false);
    }
}
