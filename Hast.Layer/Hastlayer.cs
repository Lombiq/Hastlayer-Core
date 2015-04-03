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
        /// <remarks>Point of this factory is that it returns an interface type instead of the implementation.</remarks>
        /// <param name="extensions">
        /// Extensions that can provide implementations for Hastlayer services or hook into the hardware generation pipeline.
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
             * - Synthesize hardware through vendor-specific toolchain and load it onto FPGA, together with the necessary communication 
             *   implementation (currently partially implemented with a method table).
             * - Cache hardware implementation to be able to re-configure the FPGA with it later.
             */

            await (await GetHost())
                .Run<ITransformer, IHardwareRepresentationComposer>(
                    async (transformer, hardwareRepresentationComposer) =>
                    {
                        var hardwareDescription = await transformer.Transform(assemblies, configuration);

                        if (Transformed != null) Transformed(this, new TransformedEventArgs(hardwareDescription));

                        await hardwareRepresentationComposer.Compose(hardwareDescription);

                    }, ShellName, false);

            return new HardwareRepresentation
            {
                SoftAssemblies = assemblies
            };
        }

        public async Task<T> GenerateProxy<T>(IHardwareRepresentation hardwareRepresentation, T hardwareObject) where T : class
        {
            Argument.ThrowIfNull(hardwareRepresentation, "hardwareAssembly");

            if (!hardwareRepresentation.SoftAssemblies.Contains(hardwareObject.GetType().Assembly))
            {
                throw new InvalidOperationException("The supplied type is not part of any assembly that this hardware representation was generated from.");
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
