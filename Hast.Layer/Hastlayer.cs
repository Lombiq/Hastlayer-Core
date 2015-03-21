using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hast.Common;
using Hast.Common.Configuration;
using Hast.Communication;
using Hast.Synthesis;
using Hast.Transformer;
using Lombiq.OrchardAppHost;
using Lombiq.OrchardAppHost.Configuration;
using Orchard.Environment.Configuration;
using Orchard.Validation;

namespace Hast.Layer
{
    public class Hastlayer : IHastlayer
    {
        private const string ShellName = ShellSettings.DefaultName;

        private readonly IEnumerable<Assembly> _extensions;
        private IOrchardAppHost _host;

        public event TransformedEventHandler Transformed;


        // Private so the static factory should be used.
        private Hastlayer(IEnumerable<Assembly> extensions)
        {
            _extensions = extensions;
        }

        /// <summary>
        /// Instantiates a new <see cref="IHastlayer"/> implementation.
        /// </summary>
        /// <remarks>Point of this factory is that it returns an interface type instead of the implemantation.</remarks>
        /// <param name="extensions">
        /// Extensions that can provide implementations for Hatlayer services or hook into the hardware generation pipeline.
        /// </param>
        /// <returns>A newly created <see cref="IHastlayer"/> implementation.</returns>
        public static IHastlayer Create(IEnumerable<Assembly> extensions)
        {
            Argument.ThrowIfNull(extensions, "extensions");

            return new Hastlayer(extensions);
        }


        public async Task<IHardwareAssembly> GenerateHardware(Assembly assembly, IHardwareGenerationConfiguration configuration)
        {
            /*
             * Steps to be implemented:
             * - Transform into hardware description through ITransformer.
             * - Save hardware description for re-use (cache file, stream supplied from the outside).
             * - Synthesize hardware through vendor-specific toolchain and load it onto FPGA, together with the necessary communication 
             *   implementation (currently partially implemented with a method table).
             * - Cache hardware implementation to be able to re-configure the FPGA with it later.
             */

            try
            {
                await (await GetHost())
                    .Run<ITransformer, IHardwareRepresentationComposer>(
                        async (transformer, hardwareRepresentationComposer) =>
                        {
                            var hardwareDescription = await transformer.Transform(assembly, configuration);

                            if (Transformed != null) Transformed(hardwareDescription);

                            await hardwareRepresentationComposer.Compose(hardwareDescription);

                        }, ShellName, false);
            }
            catch (NotImplementedException) // Just for testing.
            {
            }

            return new HardwareAssembly
            {
                SoftAssembly = assembly
            };
        }


        // Maybe this should return an IDisposable? E.g. close communication to FPGA, or clean up its configuration here if no other calls for
        // this type of object is alive.
        public async Task<T> GenerateProxy<T>(IHardwareAssembly hardwareAssembly, T hardwareObject) where T : class
        {
            Argument.ThrowIfNull(hardwareAssembly, "hardwareAssembly");

            if (hardwareAssembly.SoftAssembly != hardwareObject.GetType().Assembly)
            {
                throw new InvalidOperationException("The supplied type is not part of this hardware assembly.");
            }

            return await
                (await GetHost())
                .RunGet(scope => Task.Run<T>(() => scope.Resolve<IProxyGenerator>().CreateCommunicationProxy(hardwareObject)));
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
                // Everything loaded from here currently, but this needs an extension point for dynamic loading.
                ImportedExtensions = new[]
                {
                    typeof(Hastlayer).Assembly,
                    typeof(Hast.Communication.IProxyGenerator).Assembly,
                    typeof(Hast.Synthesis.IHardwareRepresentationComposer).Assembly,
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
                            "Hast.Synthesis",
                            "Hast.Transformer"
                        }.Union(_extensions.Select(extension => extension.ShortName()))
                    }
                }
            };

            _host = await OrchardAppHostFactory.StartTransientHost(settings, null, null);

            return _host;
        }
    }
}
