using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hast.Common;
using Hast.Common.Configuration;
using Hast.Common.Models;
using Hast.Communication;
using Hast.Layer.Extensibility.Events;
using Hast.Synthesis;
using Hast.Transformer;
using Lombiq.OrchardAppHost;
using Lombiq.OrchardAppHost.Configuration;
using Orchard.Environment.Configuration;
using Orchard.Validation;
using Orchard.Exceptions;
using Orchard.Logging;
using Hast.Layer.Models;

namespace Hast.Layer
{
    public class Hastlayer : IHastlayer
    {
        private const string ShellName = ShellSettings.DefaultName;

        private readonly IEnumerable<Assembly> _extensions;
        private IOrchardAppHost _host;

        public event ExecutedOnHardwareEventHandler ExecutedOnHardware;


        // Private so the static factory should be used.
        private Hastlayer(IEnumerable<Assembly> extensions)
        {
            _extensions = extensions;
        }

        /// <summary>
        /// Instantiates a new <see cref="IHastlayer"/> implementation.
        /// </summary>
        /// <remarks>Point of this factory is that it returns an interface type instead of the implementation.</remarks>
        /// <param name="extensions">
        /// Extensions that can provide implementations for Hastlayer services or hook into the hardware generation pipeline.
        /// These should be Orchard extensions.
        /// </param>
        /// <returns>A newly created <see cref="IHastlayer"/> implementation.</returns>
        public static IHastlayer Create(IEnumerable<Assembly> extensions)
        {
            Argument.ThrowIfNull(extensions, "extensions");

            return new Hastlayer(extensions);
        }


        public async Task<IHardwareRepresentation> GenerateHardware(IEnumerable<Assembly> assemblies, IHardwareGenerationConfiguration configuration)
        {
            Argument.ThrowIfNull(assemblies, "assemblies");
            if (!assemblies.Any())
            {
                throw new ArgumentException("No assemblies were specified.");
            }

            if (assemblies.Count() != assemblies.Distinct().Count())
            {
                throw new ArgumentException("The same assembly was included multiple times. Only supply each assembly to generate hardware from once.");
            }

            /*
             * Steps to be implemented:
             * - Transform into hardware description through ITransformer.
             * - Save hardware description for re-use (cache file, stream supplied from the outside).
             * - Synthesize hardware through vendor-specific toolchain and load it onto FPGA, together with the necessary 
             *   communication implementation (currently partially implemented with a member table). The implementation
             *   should be cached by the vendor tools.
             */

            try
            {
                HardwareRepresentation hardwareRepresentation = null;

                await (await GetHost())
                    .Run<ITransformer, IHardwareImplementationComposer>(
                        async (transformer, hardwareImplementationComposer) =>
                        {
                            var hardwareDescription = await transformer.Transform(assemblies, configuration);

                            var hardwareImplementation = await hardwareImplementationComposer.Compose(hardwareDescription);

                            hardwareRepresentation = new HardwareRepresentation
                            {
                                SoftAssemblies = assemblies,
                                HardwareDescription = hardwareDescription,
                                HardwareImplementation = hardwareImplementation
                            };
                        }, ShellName, false);

                return hardwareRepresentation;
            }
            catch (Exception ex)
            {
                if (ex.IsFatal()) throw;

                var message = "An error happened during generating the Hastlayer hardware representation for the following assemblies: " + string.Join(", ", assemblies.Select(assembly => assembly.FullName));
                // This should be async-await once we have C# 6.
                GetHost().Result.Run<ILoggerService>(logger => Task.Run(() => logger.Error(ex, message))).Wait();
                throw new HastlayerException(message, ex);
            }
        }

        public async Task<T> GenerateProxy<T>(IHardwareRepresentation hardwareRepresentation, T hardwareObject) where T : class
        {
            if (!hardwareRepresentation.SoftAssemblies.Contains(hardwareObject.GetType().Assembly))
            {
                throw new InvalidOperationException("The supplied type is not part of any assembly that this hardware representation was generated from.");
            }

            try
            {
                return await
                    (await GetHost())
                    .RunGet(scope => Task.Run<T>(() => scope.Resolve<IProxyGenerator>().CreateCommunicationProxy(hardwareRepresentation, hardwareObject)));
            }
            catch (Exception ex)
            {
                if (ex.IsFatal()) throw;

                var message = "An error happened during generating the Hastlayer proxy for an object of the following type: " + hardwareObject.GetType().FullName;
                // This should be async-await once we have C# 6.
                GetHost().Result.Run<ILoggerService>(logger => Task.Run(() => logger.Error(ex, message))).Wait();
                throw new HastlayerException(message, ex);
            }
        }

        public void Dispose()
        {
            if (_host == null) return;

            _host.Dispose();
            _host = null;
        }


        private async Task<IOrchardAppHost> GetHost()
        {
            if (_host != null) return _host;

            var settings = new AppHostSettings
            {
                ImportedExtensions = new[]
                {
                    typeof(Hastlayer).Assembly,
                    typeof(Hast.Communication.IProxyGenerator).Assembly,
                    typeof(Hast.Synthesis.IHardwareImplementationComposer).Assembly,
                    typeof(Hast.Transformer.ITransformer).Assembly
                }.Union(_extensions),
                DefaultShellFeatureStates = new[]
                {
                    new DefaultShellFeatureState
                    {
                        ShellName = ShellName,
                        EnabledFeatures = new[]
                        {
                            "Hast.Layer",
                            "Hast.Communication",
                            "Hast.Communication.Ethernet",
                            "Hast.Synthesis",
                            "Hast.Transformer"
                        }.Union(_extensions.Select(extension => extension.ShortName()))
                    }
                }
            };

            _host = await OrchardAppHostFactory.StartTransientHost(settings, null, null);

            await _host.Run<IHardwareExecutionEventProxy>(proxy => Task.Run(() => proxy.RegisterExecutedOnHardwareEventHandler(eventArgs =>
                {
                    if (ExecutedOnHardware != null)
                    {
                        ExecutedOnHardware(this, eventArgs);
                    }
                })));

            return _host;
        }
    }
}
