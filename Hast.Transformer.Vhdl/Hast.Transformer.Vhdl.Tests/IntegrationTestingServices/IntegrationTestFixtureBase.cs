using Hast.Layer;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Hast.Transformer.Vhdl.Tests.IntegrationTestingServices;

public abstract class IntegrationTestFixtureBase : IDisposable
{
    private readonly Lazy<Hastlayer> _host;

    private bool _disposed;
    protected HastlayerConfiguration _hostConfiguration = new();

    protected Hastlayer Host => _host.Value;

    protected IntegrationTestFixtureBase()
    {
        _hostConfiguration.Extensions = new List<Assembly>();
        _host = new Lazy<Hastlayer>(() => Hastlayer.Create(_hostConfiguration));
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    // Protected implementation of Dispose pattern.
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing) Host.Dispose();

        _disposed = true;
    }
}
