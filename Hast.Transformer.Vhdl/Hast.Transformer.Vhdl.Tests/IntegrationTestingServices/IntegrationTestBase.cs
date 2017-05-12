﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Hast.Transformer.Models;
using Hast.Transformer.Vhdl.Events;
using Hast.Transformer.Vhdl.Models;
using ICSharpCode.NRefactory.CSharp;
using Lombiq.OrchardAppHost;
using Lombiq.OrchardAppHost.Configuration;
using NUnit.Framework;
using Orchard.Events;

namespace Hast.Transformer.Vhdl.Tests.IntegrationTestingServices
{
    public abstract class IntegrationTestBase
    {
        protected List<Assembly> _requiredExtension = new List<Assembly>();
        protected Action<ContainerBuilder> _shellRegistrationBuilder;
        protected IOrchardAppHost _host;


        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
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
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            _host.Dispose();
        }
    }
}
